namespace FauluApartmentAPI.Models.Dtos;

/// <summary>
/// DTO for Unit requests
/// </summary>
public class CreateUnitDto
{
    public string UnitNumber { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public int BuildingId { get; set; }
    public int FloorNumber { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? Deposit { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal? SquareFootage { get; set; }
    public bool IsFurnished { get; set; }
    public string? Amenities { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating many similar units at once.
/// Everything except UnitNumbers is the shared "template" applied to every unit.
/// </summary>
public class BulkCreateUnitsDto
{
    public int BuildingId { get; set; }
    public List<string> UnitNumbers { get; set; } = new();

    // Shared template
    public string UnitType { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? Deposit { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal? SquareFootage { get; set; }
    public bool IsFurnished { get; set; }
    public string? Amenities { get; set; }
    public string? Notes { get; set; }
}

public class UpdateUnitDto
{
    public int Id { get; set; }
    public string? UnitNumber { get; set; }
    public string? UnitType { get; set; }
    public decimal? MonthlyRent { get; set; }
    public decimal? Deposit { get; set; }
    public int? BedroomCount { get; set; }
    public int? BathroomCount { get; set; }
    public decimal? SquareFootage { get; set; }
    public bool? IsFurnished { get; set; }
    public string? Status { get; set; }
    public string? Amenities { get; set; }
    public string? Notes { get; set; }
}

public class UnitDto
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public int BuildingId { get; set; }
    public int FloorNumber { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? Deposit { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal? SquareFootage { get; set; }
    public bool IsFurnished { get; set; }
    public string Status { get; set; } = "Vacant";
    public string? Amenities { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}