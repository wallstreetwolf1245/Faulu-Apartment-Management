using System.Text.Json;
using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;
using FauluApartmentAPI.Models;
using FauluApartmentAPI.Models.Dtos;
using FauluApartmentAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FauluApartmentAPI.Controllers;

[ApiController]
[Route("api/webhooks")]
public class MpesaController : ControllerBase
{
    private readonly IUnitRepository _unitRepository;
    private readonly ILeaseRepository _leaseRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionStatusLogRepository _txLogRepository;
    private readonly IDarajaService _darajaService;
    private readonly ILogger<MpesaController> _logger;

    public MpesaController(
        IUnitRepository unitRepository,
        ILeaseRepository leaseRepository,
        IPaymentRepository paymentRepository,
        ITransactionStatusLogRepository txLogRepository,
        IDarajaService darajaService,
        ILogger<MpesaController> logger)
    {
        _unitRepository = unitRepository;
        _leaseRepository = leaseRepository;
        _paymentRepository = paymentRepository;
        _txLogRepository = txLogRepository;
        _darajaService = darajaService;
        _logger = logger;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Existing C2B endpoints (unchanged)
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by Daraja before processing a payment.
    /// Always accept — never block a real payment at this stage.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("c2b/validation")]
    public IActionResult C2BValidation([FromBody] C2BValidationDto dto)
    {
        _logger.LogInformation(
            "C2B Validation → Phone: {Phone} | BillRef: {BillRef} | Amount: {Amount}",
            dto.MSISDN, dto.BillRefNumber, dto.TransAmount);

        return Ok(new { ResultCode = "0", ResultDesc = "Accepted" });
    }

    /// <summary>
    /// Initiates a Transaction Status query against Daraja (requires Initiator/SecurityCredential configured).
    /// This is an authenticated endpoint intended for admin/debug use only.
    /// </summary>
    [Authorize]
    [HttpPost("transactionstatus/query")]
    public async Task<IActionResult> QueryTransactionStatus([FromQuery] string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return BadRequest(ApiResponse.ErrorResponse("transactionId is required"));

        try
        {
            var result = await _darajaService.RequestTransactionStatusAsync(transactionId);
            return Ok(new { raw = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting transaction status for {TransactionId}", transactionId);
            return StatusCode(500, ApiResponse.ErrorResponse("Failed to request transaction status"));
        }
    }

    [AllowAnonymous]
    [HttpPost("transactionstatus/result")]
    public async Task<IActionResult> TransactionStatusResult([FromBody] JsonElement payload)
    {
        _logger.LogInformation("TransactionStatus Result callback received: {Payload}", payload.ToString());

        try
        {
            // Defensive parsing: look for Result.TransactionID and ResultParameters arrays
            string? transactionId = null;
            string? receipt = null;
            string? msisdn = null;
            decimal amount = 0;
            DateTime? transactionDate = null;

            if (payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("Result", out var resultEl))
            {
                if (resultEl.TryGetProperty("TransactionID", out var txEl) && txEl.ValueKind == JsonValueKind.String)
                    transactionId = txEl.GetString();

                // Look for ResultParameters -> ResultParameter (array) or ResultParameters -> ResultParameter -> Parameter
                if (resultEl.TryGetProperty("ResultParameters", out var rp) && rp.ValueKind == JsonValueKind.Object)
                {
                    // Try common shapes
                    if (rp.TryGetProperty("ResultParameter", out var rpArray) && rpArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in rpArray.EnumerateArray())
                        {
                            ExtractKeyValueFromElement(item, ref receipt, ref msisdn, ref amount, ref transactionDate);
                        }
                    }
                    else if (rp.TryGetProperty("ResultParameters", out var inner) && inner.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in inner.EnumerateArray())
                        {
                            ExtractKeyValueFromElement(item, ref receipt, ref msisdn, ref amount, ref transactionDate);
                        }
                    }
                }

                // Fallback: some providers put parameters under "Result" -> "ResultParameters" -> "ResultParameter" -> {"Key","Value"}
                if (receipt == null || amount == 0)
                {
                    // scan entire result object for likely keys
                    ScanForLikelyFields(resultEl, ref transactionId, ref receipt, ref msisdn, ref amount, ref transactionDate);
                }
            }

            // If not inside Result, scan root for likely fields
            if (transactionId == null || (receipt == null && amount == 0))
            {
                ScanForLikelyFields(payload, ref transactionId, ref receipt, ref msisdn, ref amount, ref transactionDate);
            }

            // Attempt to find matching payment
            Payment? payment = null;
            if (!string.IsNullOrWhiteSpace(receipt))
            {
                payment = await _paymentRepository.FirstOrDefaultAsync(p => p.MpesaReceiptNumber == receipt);
            }

            if (payment == null && !string.IsNullOrWhiteSpace(transactionId))
            {
                payment = await _paymentRepository.FirstOrDefaultAsync(p => p.TransactionReference == transactionId || p.CheckoutRequestId == transactionId);
            }

            if (payment != null)
            {
                // Persist raw callback into TransactionStatusLog and link to payment
                try
                {
                    var log = new TransactionStatusLog
                    {
                        PaymentId = payment.Id,
                        TransactionId = transactionId,
                        ReceiptNumber = receipt,
                        Amount = amount > 0 ? amount : null,
                        Msisdn = msisdn,
                        RawPayload = payload.ToString(),
                        ReceivedAt = DateTime.UtcNow
                    };
                    await _txLogRepository.AddAsync(log);
                    await _txLogRepository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to persist TransactionStatus log for payment {PaymentId}", payment.Id);
                }

                if (amount > 0) payment.PaidAmount = amount;
                if (transactionDate.HasValue) payment.PaidDate = transactionDate.Value;
                if (!string.IsNullOrWhiteSpace(msisdn)) payment.PayerPhone = msisdn;
                if (!string.IsNullOrWhiteSpace(receipt)) payment.MpesaReceiptNumber = receipt;

                payment.Status = "Completed";
                payment.Notes = (payment.Notes ?? string.Empty) + $" | TransactionStatus callback. TxId: {transactionId} Receipt: {receipt} Amount: {amount}";

                await _paymentRepository.UpdateAsync(payment);
                await _paymentRepository.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentId} updated from TransactionStatus callback: TxId={TxId} Receipt={Receipt}", payment.Id, transactionId, receipt);
            }
            else
            {
                _logger.LogWarning("TransactionStatus callback received but no matching payment found. TxId={TxId} Receipt={Receipt}", transactionId, receipt);
                try
                {
                    var log = new TransactionStatusLog
                    {
                        PaymentId = null,
                        TransactionId = transactionId,
                        ReceiptNumber = receipt,
                        Amount = amount > 0 ? amount : null,
                        Msisdn = msisdn,
                        RawPayload = payload.ToString(),
                        ReceivedAt = DateTime.UtcNow
                    };
                    await _txLogRepository.AddAsync(log);
                    await _txLogRepository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to persist TransactionStatus log (unmatched)");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TransactionStatus result callback");
        }

        return Ok(new { ResultCode = "0", ResultDesc = "Accepted" });
    }

    [AllowAnonymous]
    [HttpPost("transactionstatus/timeout")]
    public IActionResult TransactionStatusTimeout([FromBody] JsonElement payload)
    {
        _logger.LogWarning("TransactionStatus Timeout callback received: {Payload}", payload.ToString());
        return Ok(new { ResultCode = "0", ResultDesc = "Accepted" });
    }

    /// <summary>
    /// Called by Daraja after payment is confirmed on Safaricom's side.
    /// Matches BillRefNumber to a unit or saves as Unmatched for manual reconciliation.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("c2b/confirmation")]
    public async Task<IActionResult> C2BConfirmation(
        [FromBody] C2BConfirmationDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "C2B Confirmation → Receipt: {Receipt} | Phone: {Phone} | BillRef: {BillRef} | Amount: {Amount}",
            dto.TransID, dto.MSISDN, dto.BillRefNumber, dto.TransAmount);

        try
        {
            var amount = decimal.TryParse(dto.TransAmount, out var amt) ? amt : 0;
            var billRef = dto.BillRefNumber?.Trim() ?? string.Empty;
            var payerName = $"{dto.FirstName} {dto.MiddleName} {dto.LastName}"
                .Trim().Replace("  ", " ");

            DateTime.TryParseExact(
                dto.TransTime, "yyyyMMddHHmmss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var transactionDate);

            if (transactionDate == default) transactionDate = DateTime.UtcNow;

            // Match BillRefNumber to a unit (case-insensitive)
            var allUnits = await _unitRepository.GetAllAsync(cancellationToken);
            var matchedUnit = allUnits.FirstOrDefault(u =>
                u.UnitNumber.Equals(billRef, StringComparison.OrdinalIgnoreCase));

            Payment payment;

            if (matchedUnit != null)
            {
                var allLeases = await _leaseRepository.GetAllAsync(cancellationToken);
                var activeLease = allLeases.FirstOrDefault(l =>
                    l.UnitId == matchedUnit.Id && l.Status == "Active");

                payment = new Payment
                {
                    Amount = amount,
                    MpesaReceiptNumber = dto.TransID,
                    BillRefNumber = dto.BillRefNumber,
                    PayerPhone = dto.MSISDN,
                    PayerName = payerName,
                    PaidDate = transactionDate,
                    PaymentMethod = "M-Pesa",
                    Status = "Completed",
                    PaymentSource = "Mpesa_C2B",
                    UnitId = matchedUnit.Id,
                    LeaseId = activeLease?.Id,
                    TenantId = activeLease?.TenantId,
                    Notes = $"M-Pesa C2B. Receipt: {dto.TransID}",
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation(
                    "Matched → Unit: {Unit} | Tenant: {TenantId}",
                    matchedUnit.UnitNumber, activeLease?.TenantId);
            }
            else
            {
                payment = new Payment
                {
                    Amount = amount,
                    MpesaReceiptNumber = dto.TransID,
                    BillRefNumber = dto.BillRefNumber,
                    PayerPhone = dto.MSISDN,
                    PayerName = payerName,
                    PaidDate = transactionDate,
                    PaymentMethod = "M-Pesa",
                    Status = "Unmatched",
                    PaymentSource = "Mpesa_C2B",
                    Notes = $"Unmatched. Account ref entered: '{dto.BillRefNumber}'. Receipt: {dto.TransID}",
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogWarning(
                    "No unit matched BillRef='{BillRef}', Receipt={Receipt}",
                    dto.BillRefNumber, dto.TransID);
            }

            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing C2B confirmation. Receipt={Receipt}", dto.TransID);
        }

        return Ok(new { ResultCode = "0", ResultDesc = "Accepted" });
    }

    /// <summary>
    /// Manually assign an Unmatched payment to a unit.
    /// </summary>
    [Authorize]
    [HttpPut("payments/{paymentId}/match")]
    public async Task<IActionResult> MatchPayment(
        int paymentId, [FromBody] MatchPaymentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null)
                return NotFound(ApiResponse.ErrorResponse("Payment not found"));

            if (payment.Status != "Unmatched")
                return BadRequest(ApiResponse.ErrorResponse("Payment is already matched"));

            var unit = await _unitRepository.GetByIdAsync(dto.UnitId, cancellationToken);
            if (unit == null)
                return NotFound(ApiResponse.ErrorResponse("Unit not found"));

            var allLeases = await _leaseRepository.GetAllAsync(cancellationToken);
            var activeLease = allLeases.FirstOrDefault(l =>
                l.UnitId == dto.UnitId && l.Status == "Active");

            payment.UnitId = dto.UnitId;
            payment.LeaseId = activeLease?.Id;
            payment.TenantId = activeLease?.TenantId;
            payment.Status = "Completed";
            payment.Notes += $" | Manually matched to Unit {unit.UnitNumber}";
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse.SuccessResponse("Payment matched successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error matching payment {PaymentId}", paymentId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while matching the payment"));
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // New STK Push endpoints
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sends an STK push prompt to the tenant's phone and creates a pending payment
    /// record so the eventual callback has something to update.
    /// </summary>
    [Authorize]
    [HttpPost("stkpush/initiate")]
    public async Task<IActionResult> InitiateStkPush(
        [FromBody] StkPushInitiateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Resolve a sensible account reference from the unit number, if we can find it
            string accountReference = dto.AccountReference ?? "FauluRent";
            int? unitId = null;

            // LeaseId is now nullable — a tenant might not have a resolvable
            // active lease at request time. Only attempt the lookup if we
            // actually got one, instead of assuming it's always present.
            if (dto.LeaseId.HasValue)
            {
                var lease = await _leaseRepository.GetByIdAsync(dto.LeaseId.Value, cancellationToken);
                if (lease != null)
                {
                    unitId = lease.UnitId;
                    var unit = await _unitRepository.GetByIdAsync(lease.UnitId, cancellationToken);
                    if (unit != null && string.IsNullOrWhiteSpace(dto.AccountReference))
                    {
                        accountReference = unit.UnitNumber;
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "InitiateStkPush: LeaseId {LeaseId} provided but not found for tenant {TenantId}",
                        dto.LeaseId.Value, dto.TenantId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "InitiateStkPush: no LeaseId provided for tenant {TenantId} — payment will be created without a unit link, account reference falls back to '{AccountReference}'",
                    dto.TenantId, accountReference);
            }

            var stkRequest = new StkPushRequestDto
            {
                PhoneNumber = dto.PhoneNumber,
                Amount = dto.Amount,
                AccountReference = accountReference,
                TransactionDesc = "Rent Payment"
            };

            var stkResponse = await _darajaService.InitiateStkPushAsync(stkRequest);

            if (stkResponse.ResponseCode != "0" || string.IsNullOrEmpty(stkResponse.CheckoutRequestID))
            {
                _logger.LogWarning(
                    "STK push not accepted by Daraja: {Code} | {Desc}",
                    stkResponse.ResponseCode, stkResponse.ResponseDescription);

                return BadRequest(ApiResponse.ErrorResponse(
                    stkResponse.ResponseDescription ?? "Failed to initiate STK push"));
            }

            var notesParts = new List<string> { "STK Push initiated" };
            if (!string.IsNullOrWhiteSpace(dto.RentalPeriod)) notesParts.Add($"Period: {dto.RentalPeriod}");
            if (!string.IsNullOrWhiteSpace(dto.Notes)) notesParts.Add(dto.Notes);

            var payment = new Payment
            {
                TenantId = dto.TenantId,
                LeaseId = dto.LeaseId,
                UnitId = unitId,
                Amount = dto.Amount,
                DueDate = DateTime.UtcNow,
                PaymentMethod = "mpesa_stk",
                PaymentType = "RentPayment",
                Status = "PendingStkPush",
                PaymentSource = "Mpesa_STK",
                PayerPhone = dto.PhoneNumber,
                CheckoutRequestId = stkResponse.CheckoutRequestID,
                MerchantRequestId = stkResponse.MerchantRequestID,
                Notes = string.Join(" | ", notesParts),
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                paymentId = payment.Id,
                checkoutRequestId = stkResponse.CheckoutRequestID,
                customerMessage = stkResponse.CustomerMessage
            }, "STK push sent"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating STK push for tenant {TenantId}", dto.TenantId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while initiating the STK push"));
        }
    }

    /// <summary>
    /// Called by Daraja once the customer approves, cancels, or times out the STK prompt.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("stkpush/callback")]
    public async Task<IActionResult> StkPushCallback(
        [FromBody] StkCallbackEnvelopeDto dto, CancellationToken cancellationToken)
    {
        var callback = dto.Body.StkCallback;

        _logger.LogInformation(
            "STK Callback → CheckoutRequestID: {CheckoutId} | ResultCode: {Code} | Desc: {Desc}",
            callback.CheckoutRequestID, callback.ResultCode, callback.ResultDesc);

        try
        {
            var payment = await _paymentRepository.GetByCheckoutRequestIdAsync(
                callback.CheckoutRequestID, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning(
                    "STK callback received for unknown CheckoutRequestID: {CheckoutId}",
                    callback.CheckoutRequestID);
                // Still return success — Daraja doesn't need to know about our internal state
                return Ok(new { ResultCode = "0", ResultDesc = "Accepted" });
            }

            if (callback.ResultCode == 0)
            {
                var items = callback.CallbackMetadata?.Item ?? new List<StkCallbackItemDto>();

                var receiptNumber = GetStringValue(items, "MpesaReceiptNumber");
                var amount = GetDecimalValue(items, "Amount");
                var transactionDateRaw = GetStringValue(items, "TransactionDate");
                var phone = GetStringValue(items, "PhoneNumber");

                DateTime.TryParseExact(
                    transactionDateRaw, "yyyyMMddHHmmss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var paidDate);

                payment.Status = "Completed";
                payment.PaidAmount = amount > 0 ? amount : payment.Amount;
                payment.PaidDate = paidDate == default ? DateTime.UtcNow : paidDate;
                payment.MpesaReceiptNumber = receiptNumber;
                if (!string.IsNullOrWhiteSpace(phone)) payment.PayerPhone = phone;
                payment.Notes += $" | Confirmed. Receipt: {receiptNumber}";
            }
            else
            {
                payment.Status = "Failed";
                payment.Notes += $" | Failed: {callback.ResultDesc}";
            }

            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing STK callback for CheckoutRequestID {CheckoutId}", callback.CheckoutRequestID);
        }

        // Daraja always expects this shape regardless of what we did internally
        return Ok(new { ResultCode = "0", ResultDesc = "Accepted" });
    }

    /// <summary>
    /// Polled by the frontend while waiting for the customer to approve the STK prompt.
    /// </summary>
    [Authorize]
    [HttpGet("stkpush/status/{checkoutRequestId}")]
    public async Task<IActionResult> GetStkPushStatus(string checkoutRequestId, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByCheckoutRequestIdAsync(checkoutRequestId, cancellationToken);

        if (payment == null)
            return NotFound(ApiResponse.ErrorResponse("No payment found for this checkout request"));

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            paymentId = payment.Id,
            status = payment.Status, // PendingStkPush | Completed | Failed
            amount = payment.Amount,
            paidAmount = payment.PaidAmount,
            mpesaReceiptNumber = payment.MpesaReceiptNumber,
            notes = payment.Notes
        }, "OK"));
    }

    // ────────────────────────────────────────────────────────────────────────
    // New: Pull Transactions endpoints (reconciliation for missed C2B callbacks)
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// One-time (idempotent) registration of this shortcode for the Pull Transactions API.
    /// Safe to call repeatedly — Daraja returns "1001 ShortCode already Registered" on repeats.
    /// </summary>
    [Authorize]
    [HttpPost("pulltransactions/register")]
    public async Task<IActionResult> RegisterPullTransactions()
    {
        try
        {
            var result = await _darajaService.RegisterPullTransactionsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering Pull Transactions");
            return StatusCode(500, ApiResponse.ErrorResponse("Failed to register Pull Transactions"));
        }
    }

    /// <summary>
    /// Triggers a pull for transactions in the given window. Results arrive asynchronously
    /// at the PullTransactionsCallback endpoint below — this call only kicks off the request.
    /// </summary>
    [Authorize]
    [HttpGet("pulltransactions/query")]
    public async Task<IActionResult> QueryPullTransactions(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int offset = 0)
    {
        try
        {
            var result = await _darajaService.QueryPullTransactionsAsync(startDate, endDate, offset);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Pull Transactions for {StartDate} - {EndDate}", startDate, endDate);
            return StatusCode(500, ApiResponse.ErrorResponse("Failed to query Pull Transactions"));
        }
    }

    /// <summary>
    /// Daraja posts pulled transactions here asynchronously after a query.
    /// Reuses the same BillRefNumber-to-unit matching as C2B confirmation, and skips
    /// any receipt already on file so re-pulling an overlapping window is safe.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("pulltransactions/callback")]
    public async Task<IActionResult> PullTransactionsCallback(
        [FromBody] PullTransactionCallbackDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Pull Transactions callback: {Count} transaction(s) received", dto.Response?.Count ?? 0);

        try
        {
            var allUnits = await _unitRepository.GetAllAsync(cancellationToken);
            var allLeases = await _leaseRepository.GetAllAsync(cancellationToken);

            foreach (var tx in dto.Response ?? new List<PulledTransactionDto>())
            {
                if (string.IsNullOrWhiteSpace(tx.TransactionId))
                {
                    _logger.LogWarning("Pull Transactions callback item missing TransactionId — skipped");
                    continue;
                }

                // Skip if we already have this receipt — pull results can overlap with live callbacks
                var existing = await _paymentRepository.FirstOrDefaultAsync(
                    p => p.MpesaReceiptNumber == tx.TransactionId, cancellationToken);
                if (existing != null)
                {
                    _logger.LogInformation("Pull Transactions: receipt {Receipt} already exists — skipped", tx.TransactionId);
                    continue;
                }

                var billRef = tx.BillRefNumber?.Trim() ?? string.Empty;
                var matchedUnit = allUnits.FirstOrDefault(u =>
                    u.UnitNumber.Equals(billRef, StringComparison.OrdinalIgnoreCase));

                var activeLease = matchedUnit != null
                    ? allLeases.FirstOrDefault(l => l.UnitId == matchedUnit.Id && l.Status == "Active")
                    : null;

                DateTime.TryParseExact(
                    tx.TransTime, "yyyyMMddHHmmss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var transactionDate);

                var payerName = $"{tx.FirstName} {tx.MiddleName} {tx.LastName}".Trim().Replace("  ", " ");

                var payment = new Payment
                {
                    Amount = tx.TransAmount,
                    MpesaReceiptNumber = tx.TransactionId,
                    BillRefNumber = tx.BillRefNumber,
                    PayerPhone = tx.MSISDN,
                    PayerName = payerName,
                    PaidDate = transactionDate == default ? DateTime.UtcNow : transactionDate,
                    PaymentMethod = "M-Pesa",
                    Status = matchedUnit != null ? "Completed" : "Unmatched",
                    PaymentSource = "Mpesa_Pull",
                    UnitId = matchedUnit?.Id,
                    LeaseId = activeLease?.Id,
                    TenantId = activeLease?.TenantId,
                    Notes = matchedUnit != null
                        ? $"Recovered via Pull Transactions. Receipt: {tx.TransactionId}"
                        : $"Recovered via Pull Transactions, unmatched. Account ref: '{tx.BillRefNumber}'. Receipt: {tx.TransactionId}",
                    CreatedAt = DateTime.UtcNow
                };

                await _paymentRepository.AddAsync(payment, cancellationToken);

                if (matchedUnit == null)
                {
                    _logger.LogWarning(
                        "Pull Transactions: no unit matched BillRef='{BillRef}', Receipt={Receipt}",
                        tx.BillRefNumber, tx.TransactionId);
                }
            }

            await _paymentRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Pull Transactions callback");
        }

        return Ok(new { ResultCode = "0", ResultDesc = "Accepted" });
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers for parsing Daraja's CallbackMetadata.Item array (mixed types)
    // ────────────────────────────────────────────────────────────────────────

    private static string? GetStringValue(List<StkCallbackItemDto> items, string name)
    {
        var item = items.FirstOrDefault(i => i.Name == name);
        if (item?.Value is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                JsonValueKind.Number => je.GetRawText(),
                _ => je.ToString()
            };
        }
        return item?.Value?.ToString();
    }

    private static decimal GetDecimalValue(List<StkCallbackItemDto> items, string name)
    {
        var item = items.FirstOrDefault(i => i.Name == name);
        if (item?.Value is JsonElement je && je.ValueKind == JsonValueKind.Number)
            return je.GetDecimal();
        return 0;
    }

    private static void ExtractKeyValueFromElement(JsonElement el, ref string? receipt, ref string? msisdn, ref decimal amount, ref DateTime? transactionDate)
    {
        // Common shapes: { Key: "TransactionReceipt", Value: "ABC123" } or { "Key": "TransactionReceipt", "Value": {"Value": "..."} }
        string key = string.Empty;
        if (el.ValueKind == JsonValueKind.Object)
        {
            if (el.TryGetProperty("Key", out var k1) && k1.ValueKind == JsonValueKind.String) key = k1.GetString() ?? string.Empty;
            if (el.TryGetProperty("Name", out var k2) && k2.ValueKind == JsonValueKind.String) key = k2.GetString() ?? key;

            if (el.TryGetProperty("Value", out var val))
            {
                if (val.ValueKind == JsonValueKind.String)
                {
                    var v = val.GetString();
                    if (string.Equals(key, "TransactionReceipt", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(key, "TransactionReceiptNumber", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(key, "MpesaReceiptNumber", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(key, "ReceiptNo", StringComparison.OrdinalIgnoreCase))
                    {
                        receipt ??= v;
                    }
                    else if (string.Equals(key, "MSISDN", StringComparison.OrdinalIgnoreCase) || string.Equals(key, "PhoneNumber", StringComparison.OrdinalIgnoreCase))
                    {
                        msisdn ??= v;
                    }
                    else if (string.Equals(key, "TransactionID", StringComparison.OrdinalIgnoreCase) || string.Equals(key, "TransID", StringComparison.OrdinalIgnoreCase))
                    {
                        // sometimes the ID appears here
                        receipt ??= v;
                    }
                }
                else if (val.ValueKind == JsonValueKind.Number)
                {
                    if (string.Equals(key, "TransactionAmount", StringComparison.OrdinalIgnoreCase) || string.Equals(key, "Amount", StringComparison.OrdinalIgnoreCase))
                    {
                        amount = val.GetDecimal();
                    }
                }
            }
        }
    }

    private static void ScanForLikelyFields(JsonElement el, ref string? transactionId, ref string? receipt, ref string? msisdn, ref decimal amount, ref DateTime? transactionDate)
    {
        // Recursively search for common property names
        if (el.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in el.EnumerateObject())
            {
                var name = prop.Name;
                if (string.Equals(name, "TransactionID", StringComparison.OrdinalIgnoreCase) && prop.Value.ValueKind == JsonValueKind.String)
                    transactionId ??= prop.Value.GetString();
                else if ((string.Equals(name, "TransactionReceipt", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "MpesaReceiptNumber", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "ReceiptNo", StringComparison.OrdinalIgnoreCase)) && prop.Value.ValueKind == JsonValueKind.String)
                    receipt ??= prop.Value.GetString();
                else if ((string.Equals(name, "MSISDN", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "PhoneNumber", StringComparison.OrdinalIgnoreCase)) && prop.Value.ValueKind == JsonValueKind.String)
                    msisdn ??= prop.Value.GetString();
                else if ((string.Equals(name, "Amount", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "TransactionAmount", StringComparison.OrdinalIgnoreCase)) && prop.Value.ValueKind == JsonValueKind.Number)
                    amount = prop.Value.GetDecimal();

                // dive deeper
                ScanForLikelyFields(prop.Value, ref transactionId, ref receipt, ref msisdn, ref amount, ref transactionDate);
            }
        }
        else if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in el.EnumerateArray())
                ScanForLikelyFields(item, ref transactionId, ref receipt, ref msisdn, ref amount, ref transactionDate);
        }
    }
}