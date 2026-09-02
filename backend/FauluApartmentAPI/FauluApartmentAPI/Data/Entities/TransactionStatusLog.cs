namespace FauluApartmentAPI.Data.Entities;

public class TransactionStatusLog : BaseEntity
{
    public int? PaymentId { get; set; }
    public string? TransactionId { get; set; }
    public string? ReceiptNumber { get; set; }
    public decimal? Amount { get; set; }
    public string? Msisdn { get; set; }
    public string RawPayload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual Payment? Payment { get; set; }
}
