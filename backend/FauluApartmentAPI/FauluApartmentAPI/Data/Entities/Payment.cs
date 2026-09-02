namespace FauluApartmentAPI.Data.Entities;

/// <summary>
/// Represents a rent payment or invoice
/// </summary>
public class Payment : BaseEntity
{
    public int? TenantId { get; set; }
    public int? LeaseId { get; set; }
    public int? UnitId { get; set; }

    public decimal Amount { get; set; }
    public decimal? LateFees { get; set; } = 0;
    public decimal? PaidAmount { get; set; }

    public string PaymentType { get; set; } = "RentPayment";
    public string Status { get; set; } = "Pending";
    public string PaymentSource { get; set; } = "Manual";

    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }

    public string? PaymentMethod { get; set; } = "Bank";
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }

    // M-Pesa C2B fields
    public string? MpesaReceiptNumber { get; set; }
    public string? BillRefNumber { get; set; }
    public string? PayerPhone { get; set; }
    public string? PayerName { get; set; }

    // M-Pesa STK Push tracking fields
    public string? CheckoutRequestId { get; set; }
    public string? MerchantRequestId { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual Lease? Lease { get; set; }
    public virtual Unit? Unit { get; set; }
}