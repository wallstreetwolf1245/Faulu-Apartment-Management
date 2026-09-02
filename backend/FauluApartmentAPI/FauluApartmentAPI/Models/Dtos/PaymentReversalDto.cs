namespace FauluApartmentAPI.Models.Dtos;

public class CreateReversalDto
{
    public string Reason { get; set; } = string.Empty;
}

public class PaymentReversalDto
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public int TenantId { get; set; }
    public int? BuildingId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string Reason { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? PaymentReferenceId { get; set; }
    public string? TenantName { get; set; }
    public string? UnitNumber { get; set; }
}

public class UpdateReversalStatusDto
{
    public string Status { get; set; } = "Approved"; // Approved or Rejected
}
