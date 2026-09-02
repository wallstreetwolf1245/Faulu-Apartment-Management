namespace FauluApartmentAPI.Models.Dtos;

/// <summary>
/// DTO for ServiceRequest requests
/// </summary>
public class CreateServiceRequestDto
{
    public int TenantId { get; set; }
    public int? UnitId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
}

public class UpdateServiceRequestDto
{
    public int Id { get; set; }
    public string? Status { get; set; }
    public int? AssignedManagerId { get; set; }
    public string? Response { get; set; }
    public double? Rating { get; set; }
    public string? Feedback { get; set; }
}

public class ServiceRequestDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int? UnitId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public string Status { get; set; } = "Open";
    public DateTime RequestDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? AssignedManagerId { get; set; }
    public string? Response { get; set; }
    public double? Rating { get; set; }
    public string? Feedback { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
