namespace FauluApartmentAPI.Models.Dtos;

/// <summary>
/// DTO for Building requests
/// </summary>
public class CreateBuildingDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalUnits { get; set; }
    public int OwnerId { get; set; }
    public decimal? PropertyValue { get; set; }
    public int? YearBuilt { get; set; }
}

public class UpdateBuildingDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Description { get; set; }
    public int? TotalUnits { get; set; }
    public decimal? PropertyValue { get; set; }
}

public class BuildingDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Kenya";
    public string? Description { get; set; }
    public int TotalUnits { get; set; }
    public int OwnerId { get; set; }
    public decimal? PropertyValue { get; set; }
    public int OccupancyRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
