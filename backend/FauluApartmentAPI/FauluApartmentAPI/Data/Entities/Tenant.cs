namespace FauluApartmentAPI.Data.Entities;

/// <summary>
/// Represents a tenant/occupant in the system
/// </summary>
public class Tenant : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string IdentificationType { get; set; } = "NationalID"; // NationalID, Passport, etc.
    public DateTime DateOfBirth { get; set; }
    public string? Occupation { get; set; }
    public string? Employer { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Evicted
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<Lease> Leases { get; set; } = new List<Lease>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
