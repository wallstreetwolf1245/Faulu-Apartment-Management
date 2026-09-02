namespace FauluApartmentAPI.Models.Dtos;

public class CreateTenantDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string IdentificationType { get; set; } = "NationalID";
    public DateTime DateOfBirth { get; set; }
    public string? Occupation { get; set; }
    public string? Employer { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public int? BuildingId { get; set; }
    public int? UnitId { get; set; }
    public DateTime? MoveInDate { get; set; }
    public decimal? RentAmount { get; set; }
}

public class UpdateTenantDto
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Occupation { get; set; }
    public string? Employer { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }

    // ── Lease reassignment fields (new) ──────────────────────────────────
    // When UnitId is provided and differs from the tenant's current active
    // lease's unit, the controller ends the old lease and starts a new one
    // on the new unit. When UnitId matches the current unit (or is omitted),
    // RentAmount/MoveInDate just update the existing active lease in place.
    public int? UnitId { get; set; }
    public decimal? RentAmount { get; set; }
    public DateTime? MoveInDate { get; set; }
}

public class TenantDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string IdentificationType { get; set; } = "NationalID";
    public DateTime DateOfBirth { get; set; }
    public string? Occupation { get; set; }
    public string? Employer { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Lease-derived fields (from active lease)
    public int? ActiveLeaseId { get; set; }
    public int? UnitId { get; set; }
    public string? UnitNumber { get; set; }
    public string? UnitType { get; set; }
    public int? BuildingId { get; set; }
    public string? PropertyName { get; set; }
    public decimal? RentAmount { get; set; }
    public DateTime? MoveInDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public string? LeaseStatus { get; set; }
}