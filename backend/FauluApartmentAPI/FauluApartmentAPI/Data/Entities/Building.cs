namespace FauluApartmentAPI.Data.Entities;

/// <summary>
/// Represents a building/property in the system
/// </summary>
public class Building : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Kenya";
    public string? Description { get; set; }
    public int TotalUnits { get; set; }
    public int OwnerId { get; set; }
    public decimal? PropertyValue { get; set; }
    public DateTime? YearBuilt { get; set; }

    // Navigation properties
    public virtual User? Owner { get; set; }
    public virtual ICollection<Unit> Units { get; set; } = new List<Unit>();
    public virtual ICollection<MaintenanceOrder> MaintenanceOrders { get; set; } = new List<MaintenanceOrder>();
}
