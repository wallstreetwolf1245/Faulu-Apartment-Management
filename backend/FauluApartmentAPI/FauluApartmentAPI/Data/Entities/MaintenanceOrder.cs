namespace FauluApartmentAPI.Data.Entities;

/// <summary>
/// Represents a maintenance work order
/// </summary>
public class MaintenanceOrder : BaseEntity
{
    public int BuildingId { get; set; }
    public int? UnitId { get; set; }
    public int? TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    public string Status { get; set; } = "Open"; // Open, InProgress, Completed, OnHold, Cancelled
    public string Category { get; set; } = string.Empty; // Plumbing, Electrical, Painting, etc.
    public DateTime ReportedDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? AssignedVendor { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Building? Building { get; set; }
    public virtual Unit? Unit { get; set; }
    public virtual Tenant? Tenant { get; set; }
}