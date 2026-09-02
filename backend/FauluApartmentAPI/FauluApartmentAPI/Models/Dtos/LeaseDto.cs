namespace FauluApartmentAPI.Models.Dtos;

/// <summary>
/// DTO for Lease requests
/// </summary>
public class CreateLeaseDto
{
    public int TenantId { get; set; }
    public int UnitId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal? SecondaryDeposit { get; set; }
    public string LeaseTermType { get; set; } = "12-Months";
    public string RenewalPolicy { get; set; } = "Auto";
    public bool AllowsPets { get; set; }
    public string? LeaseDocument { get; set; }
}

public class UpdateLeaseDto
{
    public int Id { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MonthlyRent { get; set; }
    public string? Status { get; set; }
    public string? RenewalPolicy { get; set; }
    public bool? AllowsPets { get; set; }
    public string? Notes { get; set; }
}

public class LeaseDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int UnitId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal? SecondaryDeposit { get; set; }
    public string Status { get; set; } = "Active";
    public string LeaseTermType { get; set; } = "12-Months";
    public string RenewalPolicy { get; set; } = "Auto";
    public bool AllowsPets { get; set; }
    public string? LeaseDocument { get; set; }
    public DateTime? SignedDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
