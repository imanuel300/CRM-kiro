using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// יומן הודעות - תיעוד כל הודעה שנשלחה כולל תאריך, נמען, תוכן וסטטוס
/// </summary>
public class NotificationLog : BaseEntity
{
    public int CandidacyId { get; set; }
    public int? TemplateId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Candidacy Candidacy { get; set; } = null!;
    public NotificationTemplate? Template { get; set; }
}
