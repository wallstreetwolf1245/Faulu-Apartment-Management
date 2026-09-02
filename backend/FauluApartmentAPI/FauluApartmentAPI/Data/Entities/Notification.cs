namespace FauluApartmentAPI.Data.Entities;

/// <summary>
/// Represents a notification to users
/// </summary>
public class Notification : BaseEntity
{
    public int RecipientUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = "Info"; // Info, Warning, Error, RentDue, LeaseExpiry, MaintenanceUpdate
    public string Status { get; set; } = "Unread"; // Unread, Read, Archived
    public DateTime SentDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReadDate { get; set; }
    public string? RelatedEntityType { get; set; } // Payment, Lease, ServiceRequest, etc.
    public int? RelatedEntityId { get; set; }
    public bool SendEmail { get; set; } = true;
    public bool SendSms { get; set; } = false;
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }

    // Navigation properties - Add later if needed for recipient user reference
}
