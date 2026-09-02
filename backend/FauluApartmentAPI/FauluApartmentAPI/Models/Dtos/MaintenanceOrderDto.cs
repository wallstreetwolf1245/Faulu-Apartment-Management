namespace FauluApartmentAPI.Models.Dtos;

/// <summary>
/// DTO for MaintenanceOrder requests
/// </summary>
public class CreateMaintenanceOrderDto
{
    public int BuildingId { get; set; }
    public int? UnitId { get; set; }
    public int TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public string Category { get; set; } = string.Empty;
    public decimal? EstimatedCost { get; set; }
    public string? AssignedVendor { get; set; }
}

public class UpdateMaintenanceOrderDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public decimal? ActualCost { get; set; }
    public string? AssignedVendor { get; set; }
    public string? Notes { get; set; }
}
public class MaintenanceOrderDto
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public int? UnitId { get; set; }
    public int? TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public string Status { get; set; } = "Open";
    public string Category { get; set; } = string.Empty;
    public DateTime ReportedDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? AssignedVendor { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}