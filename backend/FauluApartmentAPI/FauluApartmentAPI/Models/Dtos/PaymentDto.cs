namespace FauluApartmentAPI.Models.Dtos;

/// <summary>
/// DTO for Payment requests
/// </summary>
public class CreatePaymentDto
{
    public int TenantId { get; set; }
    public int? LeaseId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string PaymentType { get; set; } = "RentPayment";
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
}

public class RecordPaymentDto
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Bank";
    public string? TransactionReference { get; set; }
}

public class UpdatePaymentDto
{
    public int Id { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; }
    public decimal? LateFees { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public DateTime? PaidDate { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public int? TenantId { get; set; }
    public int? LeaseId { get; set; }
    public int? UnitId { get; set; }
    public decimal Amount { get; set; }
    public decimal? LateFees { get; set; }
    public string PaymentType { get; set; } = "RentPayment";
    public string Status { get; set; } = "Pending";
    public string PaymentSource { get; set; } = "Manual";
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? MpesaReceiptNumber { get; set; }
    public string? BillRefNumber { get; set; }
    public string? PayerPhone { get; set; }
    public string? PayerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Logs a payment that already happened, in full — cash handed over, a bank
/// transfer already confirmed, etc. No due date required.
/// </summary>
public class RecordFullPaymentDto
{
    public int TenantId { get; set; }
    public int? LeaseId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidDate { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string PaymentType { get; set; } = "RentPayment";
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
    public string? RentalPeriod { get; set; }
}