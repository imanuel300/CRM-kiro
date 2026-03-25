using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// תבנית דיוור - הגדרת הודעה אוטומטית הנשלחת למועמד בעקבות אירוע
/// </summary>
public class NotificationTemplate : BaseEntity
{
    public int OrgUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// גוף ההודעה עם משתנים דינמיים: {{CandidateName}}, {{Status}}, {{Date}}
    /// </summary>
    public string Body { get; set; } = string.Empty;

    public NotificationChannel Channel { get; set; } = NotificationChannel.Email;
    public TriggerEventType TriggerEvent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}
