using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// רשומת יומן ביקורת - תיעוד כל פעולה שמשתמש מבצע במערכת
/// כולל תאריך, שעה, משתמש וסוג פעולה
/// </summary>
public class AuditLogEntry : BaseEntity
{
    /// <summary>מזהה המשתמש שביצע את הפעולה</summary>
    public int UserId { get; set; }

    /// <summary>סוג הפעולה (Create, Update, Delete, ChangeStatus, View, SendNotification)</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>סוג הישות שעליה בוצעה הפעולה</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>מזהה הישות שעליה בוצעה הפעולה</summary>
    public int? EntityId { get; set; }

    /// <summary>מזהה יחידה ארגונית</summary>
    public int? OrgUnitId { get; set; }

    /// <summary>חותמת זמן של הפעולה</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>פרטים נוספים על הפעולה</summary>
    public string? Details { get; set; }
}
