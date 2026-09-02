using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using FauluApartmentAPI.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace FauluApartmentAPI.Services;

public class DarajaService : IDarajaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DarajaService> _logger;

    private const string TokenCacheKey = "DarajaAccessToken";

    public DarajaService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<DarajaService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
    }

    private string BaseUrl => (_configuration["Daraja:Environment"] ?? "sandbox") == "production"
        ? "https://api.safaricom.co.ke"
        : "https://sandbox.safaricom.co.ke";

    private bool IsProduction => (_configuration["Daraja:Environment"] ?? "sandbox") == "production";

    public async Task<string> GetAccessTokenAsync()
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
            return cachedToken;

        var consumerKey = _configuration["Daraja:ConsumerKey"];
        var consumerSecret = _configuration["Daraja:ConsumerSecret"];
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var response = await client.GetAsync($"{BaseUrl}/oauth/v1/generate?grant_type=client_credentials");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<DarajaTokenResponse>(content);

        var token = tokenResponse?.access_token ?? string.Empty;
        var expiresIn = int.TryParse(tokenResponse?.expires_in, out var exp) ? exp : 3600;

        // Cache with 60s buffer to avoid edge-case expiry
        _cache.Set(TokenCacheKey, token, TimeSpan.FromSeconds(expiresIn - 60));
        _logger.LogInformation("Daraja token refreshed, expires in {ExpiresIn}s", expiresIn);

        return token;
    }

    public async Task<bool> RegisterC2BUrlsAsync()
    {
        try
        {
            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                ShortCode = _configuration["Daraja:BusinessShortCode"],
                ResponseType = "Completed",
                ConfirmationURL = _configuration["Daraja:ConfirmationUrl"],
                ValidationURL = _configuration["Daraja:ValidationUrl"]
            };

            // Log exactly what we're about to send Safaricom — this is the only way to
            // know for certain whether a stale/mismatched ngrok URL is being registered,
            // since Daraja sandbox gives no way to query what's currently on file.
            _logger.LogInformation(
                "C2B URL registration payload: ShortCode={ShortCode}, ConfirmationURL={ConfirmationUrl}, ValidationURL={ValidationUrl}",
                payload.ShortCode, payload.ConfirmationURL, payload.ValidationURL);

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // FIX: Safaricom's current docs list /mpesa/c2b/v2/registerurl for BOTH
            // sandbox and production — v1 is the deprecated path. There is no
            // environment-based split anymore; always use v2.
            const string registerUrlPath = "/mpesa/c2b/v2/registerurl";

            var response = await client.PostAsync($"{BaseUrl}{registerUrlPath}", content);
            var body = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("C2B URL registration ({Path}): {Status} → {Body}", registerUrlPath, response.StatusCode, body);

            if (response.IsSuccessStatusCode)
                return true;

            // errorCode 500.003.1001 covers TWO different scenarios that must not be
            // treated the same way:
            //   1. "Urls are already registered." — harmless; URLs are already on file
            //      exactly as we'd register them. Safe to treat as success.
            //   2. "Duplicate notification info, SP ID is xxxxx, correlator is xxxxxx"
            //      — this shortcode has URLs registered on the older aggregator/MPLS
            //      (Broker) platform, and Daraja is refusing to register at all until
            //      those are deleted via a request to apisupport@safaricom.co.ke.
            //      Treating this as success would silently hide a real registration
            //      failure — confirmations/validations would never arrive even though
            //      the logs say "registered successfully".
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("errorCode", out var errorCodeProp))
                {
                    var errorCode = errorCodeProp.GetString();
                    var errorMessage = doc.RootElement.TryGetProperty("errorMessage", out var msgProp)
                        ? msgProp.GetString() ?? string.Empty
                        : string.Empty;

                    if (errorCode == "500.003.1001")
                    {
                        var isAggregatorConflict = errorMessage.Contains("Duplicate notification info", StringComparison.OrdinalIgnoreCase);

                        if (isAggregatorConflict)
                        {
                            _logger.LogError(
                                "Daraja rejected C2B URL registration due to an aggregator/MPLS platform conflict " +
                                "(errorCode={ErrorCode}, errorMessage={ErrorMessage}). Your shortcode has URLs " +
                                "registered on the older Broker platform — email apisupport@safaricom.co.ke to " +
                                "have those deleted before Daraja will accept registration. Confirmation/Validation " +
                                "callbacks will NOT fire until this is resolved.",
                                errorCode, errorMessage);
                            return false;
                        }

                        _logger.LogInformation(
                            "Daraja reports URLs already registered as-is (errorCode={ErrorCode}, errorMessage={ErrorMessage}) — treating as already-registered.",
                            errorCode, errorMessage);
                        return true;
                    }
                }
            }
            catch
            {
                // ignore parse errors and fall through to return false
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register C2B URLs with Daraja");
            return false;
        }
    }

    public async Task<StkPushResponseDto> InitiateStkPushAsync(StkPushRequestDto request)
    {
        var token = await GetAccessTokenAsync();
        var stkShortCode = _configuration["Daraja:StkShortCode"];
        var shortCode = !string.IsNullOrWhiteSpace(stkShortCode)
            ? stkShortCode
            : _configuration["Daraja:BusinessShortCode"] ?? string.Empty;
        var passKey = _configuration["Daraja:PassKey"] ?? string.Empty;
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        var password = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{shortCode}{passKey}{timestamp}"));

        var phone = NormalizePhoneNumber(request.PhoneNumber);

        var payload = new
        {
            BusinessShortCode = shortCode,
            Password = password,
            Timestamp = timestamp,
            TransactionType = "CustomerPayBillOnline",
            Amount = (long)request.Amount, // Daraja sandbox rejects decimals on this field
            PartyA = phone,
            PartyB = shortCode,
            PhoneNumber = phone,
            CallBackURL = _configuration["Daraja:StkCallbackUrl"],
            AccountReference = string.IsNullOrWhiteSpace(request.AccountReference)
                ? "FauluRent"
                : request.AccountReference,
            TransactionDesc = request.TransactionDesc
        };

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{BaseUrl}/mpesa/stkpush/v1/processrequest", content);
        var body = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("STK Push initiate: {Status} → {Body}", response.StatusCode, body);

        var result = JsonSerializer.Deserialize<StkPushResponseDto>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? new StkPushResponseDto
        {
            ResponseCode = "1",
            ResponseDescription = "Failed to parse Daraja response"
        };
    }

    public async Task<StkPushQueryResponseDto> QueryStkPushStatusAsync(string checkoutRequestId)
    {
        var token = await GetAccessTokenAsync();
        var stkShortCode = _configuration["Daraja:StkShortCode"];
        var shortCode = !string.IsNullOrWhiteSpace(stkShortCode)
            ? stkShortCode
            : _configuration["Daraja:BusinessShortCode"] ?? string.Empty;
        var passKey = _configuration["Daraja:PassKey"] ?? string.Empty;
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        var password = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{shortCode}{passKey}{timestamp}"));

        var payload = new
        {
            BusinessShortCode = shortCode,
            Password = password,
            Timestamp = timestamp,
            CheckoutRequestID = checkoutRequestId
        };

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{BaseUrl}/mpesa/stkpushquery/v1/query", content);
        var body = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("STK Push query: {Status} → {Body}", response.StatusCode, body);

        var result = JsonSerializer.Deserialize<StkPushQueryResponseDto>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? new StkPushQueryResponseDto
        {
            ResultCode = "1",
            ResultDesc = "Failed to parse Daraja response"
        };
    }

    public async Task<string> RequestTransactionStatusAsync(string transactionId)
    {
        var token = await GetAccessTokenAsync();
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Prepare SecurityCredential: prefer explicit config, fall back to encrypting InitiatorPassword
        var securityCredential = _configuration["Daraja:SecurityCredential"];
        if (string.IsNullOrWhiteSpace(securityCredential))
        {
            var certPath = _configuration["Daraja:PublicCertPath"];
            var initiatorPassword = _configuration["Daraja:InitiatorPassword"];
            if (!string.IsNullOrWhiteSpace(certPath) && !string.IsNullOrWhiteSpace(initiatorPassword) && File.Exists(certPath))
            {
                try
                {
                    var cert = new X509Certificate2(certPath);
                    using var rsa = cert.GetRSAPublicKey();
                    var bytes = Encoding.UTF8.GetBytes(initiatorPassword);
                    var encrypted = rsa.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);
                    securityCredential = Convert.ToBase64String(encrypted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate SecurityCredential from cert at {CertPath}", certPath);
                    securityCredential = string.Empty;
                }
            }
        }

        var payload = new
        {
            Initiator = _configuration["Daraja:InitiatorName"] ?? "",
            SecurityCredential = securityCredential ?? string.Empty,
            CommandID = "TransactionStatusQuery",
            TransactionID = transactionId,
            PartyA = _configuration["Daraja:BusinessShortCode"] ?? string.Empty,
            IdentifierType = "4",
            ResultURL = _configuration["Daraja:TransactionStatusResultUrl"] ?? string.Empty,
            QueueTimeOutURL = _configuration["Daraja:TransactionStatusQueueTimeoutUrl"] ?? string.Empty,
            Remarks = "Query",
            Occasion = string.Empty
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{BaseUrl}/mpesa/transactionstatus/v1/query", content);
        var body = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("TransactionStatus request: {Status} → {Body}", response.StatusCode, body);

        return body;
    }

    // NOTE: RegisterTransactionStatusUrlsAsync was removed — Daraja has no
    // /mpesa/transactionstatus/v1/registerurl endpoint (it was always returning
    // 404.001.01 "Resource not found"). TransactionStatus doesn't use a
    // pre-registered URL; ResultURL/QueueTimeOutURL are passed inline on every
    // call in RequestTransactionStatusAsync above, which already does this correctly.

    // ────────────────────────────────────────────────────────────────────────
    // New: Pull Transactions (reconciliation API for missed C2B callbacks)
    // ────────────────────────────────────────────────────────────────────────

    public async Task<PullTransactionRegisterResponseDto> RegisterPullTransactionsAsync()
    {
        var token = await GetAccessTokenAsync();
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new PullTransactionRegisterRequestDto
        {
            ShortCode = _configuration["Daraja:BusinessShortCode"] ?? string.Empty,
            RequestType = "Pull",
            NominatedNumber = _configuration["Daraja:PullNominatedNumber"] ?? string.Empty,
            CallBackURL = _configuration["Daraja:PullCallbackUrl"] ?? string.Empty
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{BaseUrl}/pulltransactions/v1/register", content);
        var body = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("Pull Transactions registration: {Status} → {Body}", response.StatusCode, body);

        var result = JsonSerializer.Deserialize<PullTransactionRegisterResponseDto>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // "1001" ("ShortCode already Registered") is expected on repeat calls — not an error.
        return result ?? new PullTransactionRegisterResponseDto
        {
            ResponseStatus = "error",
            ResponseDescription = "Failed to parse Daraja response"
        };
    }

    public async Task<PullTransactionQueryResponseDto> QueryPullTransactionsAsync(DateTime startDate, DateTime endDate, int offset = 0)
    {
        var token = await GetAccessTokenAsync();
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new PullTransactionQueryRequestDto
        {
            ShortCode = _configuration["Daraja:BusinessShortCode"] ?? string.Empty,
            StartDate = startDate.ToString("yyyy-MM-dd HH:mm:ss"),
            EndDate = endDate.ToString("yyyy-MM-dd HH:mm:ss"),
            OffSetValue = offset.ToString()
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Daraja's docs list this as a GET with a JSON body. HttpClient.GetAsync can't
        // carry a body, so build the request message directly — this matches what every
        // working Pull Transactions implementation actually does.
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/pulltransactions/v1/query")
        {
            Content = content
        };
        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("Pull Transactions query: {Status} → {Body}", response.StatusCode, body);

        var result = JsonSerializer.Deserialize<PullTransactionQueryResponseDto>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? new PullTransactionQueryResponseDto
        {
            ResponseCode = "error",
            ResponseMessage = "Failed to parse Daraja response"
        };
    }

    /// <summary>
    /// Daraja requires format 2547XXXXXXXX (no leading 0 or +).
    /// </summary>
    private static string NormalizePhoneNumber(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("0") && digits.Length == 10)
            return "254" + digits[1..];

        if (digits.StartsWith("254") && digits.Length == 12)
            return digits;

        if (digits.StartsWith("7") && digits.Length == 9)
            return "254" + digits;

        return digits; // fall through — let Daraja reject it with a clear error if still malformed
    }
}