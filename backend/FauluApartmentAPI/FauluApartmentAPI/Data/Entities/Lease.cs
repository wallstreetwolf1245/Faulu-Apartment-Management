namespace FauluApartmentAPI.Data.Entities;

/// <summary>
/// Represents a lease agreement between tenant and unit
/// </summary>
public class Lease : BaseEntity
{
    public int TenantId { get; set; }
    public int UnitId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal? SecondaryDeposit { get; set; }
    public string Status { get; set; } = "Active"; // Active, Expired, Renewed, Terminated, Pending
    public string LeaseTermType { get; set; } = "12-Months"; // 6-Months, 12-Months, 24-Months, etc.
    public string RenewalPolicy { get; set; } = "Auto"; // Auto, Manual, None
    public bool AllowsPets { get; set; } = false;
    public string? LeaseDocument { get; set; } // Path or URL to PDF
    public DateTime? SignedDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual Unit? Unit { get; set; }
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
