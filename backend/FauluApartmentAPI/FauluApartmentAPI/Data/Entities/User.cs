using Microsoft.AspNetCore.Identity;

namespace FauluApartmentAPI.Data.Entities;

/// <summary>
/// Application user entity extending IdentityUser
/// </summary>
public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumberVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Building> OwnedBuildings { get; set; } = new List<Building>();
    public virtual ICollection<ServiceRequest> CreatedServiceRequests { get; set; } = new List<ServiceRequest>();
}
