namespace FauluApartmentAPI.Data.Entities;

/// <summary>
/// Represents a tenant's service request
/// </summary>
public class ServiceRequest : BaseEntity
{
    public int TenantId { get; set; }
    public int? UnitId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Maintenance, Cleaning, Security, etc.
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    public string Status { get; set; } = "Open"; // Open, InProgress, Completed, Resolved, Cancelled
    public DateTime RequestDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? AssignedManagerId { get; set; }
    public string? Response { get; set; }
    public double? Rating { get; set; } // 1-5 star rating
    public string? Feedback { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual Unit? Unit { get; set; }
    public virtual User? AssignedManager { get; set; }
}
