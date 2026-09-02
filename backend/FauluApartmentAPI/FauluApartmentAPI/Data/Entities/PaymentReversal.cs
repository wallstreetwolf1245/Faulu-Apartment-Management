using System.ComponentModel.DataAnnotations;

namespace FauluApartmentAPI.Data.Entities;

/// <summary>
/// Represents a requested reversal for an existing payment
/// </summary>
public class PaymentReversal : BaseEntity
{
    public int PaymentId { get; set; }
    public int TenantId { get; set; }
    public int? BuildingId { get; set; }

    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Pending | Approved | Rejected
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Copy of original payment's transaction reference or MPESA receipt
    /// </summary>
    [MaxLength(200)]
    public string? PaymentReferenceId { get; set; }

    // Navigation
    public virtual Payment? Payment { get; set; }
}
