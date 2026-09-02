namespace FauluApartmentAPI.Data.Entities;

/// <summary>
/// Represents a unit/apartment within a building
/// </summary>
public class Unit : BaseEntity
{
    public string UnitNumber { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty; // e.g., "1-Bedroom", "2-Bedroom", "Studio"
    public int BuildingId { get; set; }
    public int FloorNumber { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? Deposit { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal? SquareFootage { get; set; }
    public bool IsFurnished { get; set; }
    public string Status { get; set; } = "Vacant"; // Vacant, Occupied, UnderMaintenance
    public string? Amenities { get; set; } // JSON or comma-separated
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Building? Building { get; set; }
    public virtual ICollection<Lease> Leases { get; set; } = new List<Lease>();
}
