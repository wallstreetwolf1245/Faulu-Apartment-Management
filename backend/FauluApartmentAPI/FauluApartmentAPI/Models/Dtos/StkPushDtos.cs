using System.Text.Json.Serialization;

namespace FauluApartmentAPI.Models.Dtos;

// ── Request from frontend to initiate a push ────────────────────────────────
public class StkPushInitiateDto
{
    public int TenantId { get; set; }

    // Nullable: a tenant may not have a resolvable active lease at request time
    // (e.g. frontend couldn't find one), and previously sending JSON null here
    // against a non-nullable int caused model binding to fail outright with a 400.
    public int? LeaseId { get; set; }

    public decimal Amount { get; set; }
    public string PhoneNumber { get; set; } = string.Empty; // e.g. 2547XXXXXXXX or 07XXXXXXXX
    public string? AccountReference { get; set; } // e.g. unit number
    public string? Notes { get; set; }
    public string RentalPeriod { get; set; } = string.Empty;
}

// ── What we send to Daraja ───────────────────────────────────────────────────
public class StkPushRequestDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AccountReference { get; set; } = string.Empty;
    public string TransactionDesc { get; set; } = "Rent Payment";
}

// ── Daraja's immediate ack response to the initiate call ────────────────────
public class StkPushResponseDto
{
    public string? MerchantRequestID { get; set; }
    public string? CheckoutRequestID { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseDescription { get; set; }
    public string? CustomerMessage { get; set; }
}

// ── Daraja's response to a status query ──────────────────────────────────────
public class StkPushQueryResponseDto
{
    public string? ResponseCode { get; set; }
    public string? ResponseDescription { get; set; }
    public string? MerchantRequestID { get; set; }
    public string? CheckoutRequestID { get; set; }
    public string? ResultCode { get; set; }
    public string? ResultDesc { get; set; }
}

// ── Shape of the async callback Safaricom POSTs to your CallbackURL ─────────
public class StkCallbackEnvelopeDto
{
    [JsonPropertyName("Body")]
    public StkCallbackBodyDto Body { get; set; } = new();
}

public class StkCallbackBodyDto
{
    [JsonPropertyName("stkCallback")]
    public StkCallbackDto StkCallback { get; set; } = new();
}

public class StkCallbackDto
{
    public string MerchantRequestID { get; set; } = string.Empty;
    public string CheckoutRequestID { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string ResultDesc { get; set; } = string.Empty;
    public StkCallbackMetadataDto? CallbackMetadata { get; set; }
}

public class StkCallbackMetadataDto
{
    public List<StkCallbackItemDto> Item { get; set; } = new();
}

public class StkCallbackItemDto
{
    public string Name { get; set; } = string.Empty;

    // Daraja mixes strings and numbers here (Amount is a number, MpesaReceiptNumber is a string),
    // so we accept it as a raw JsonElement-friendly object and parse on demand.
    public object? Value { get; set; }
}