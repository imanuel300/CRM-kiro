using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// יומן קריאות API חיצוני - תיעוד כל קריאה לממשק הקליטה
/// כולל תאריך, מזהה מערכת חיצונית, תוצאה ופרטי שגיאה
/// </summary>
public class ApiCallLog : BaseEntity
{
    /// <summary>
    /// מזהה המערכת החיצונית (מתוך ה-API Key)
    /// </summary>
    public string ExternalSystemId { get; set; } = string.Empty;

    /// <summary>
    /// נתיב הבקשה (Endpoint)
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// שיטת HTTP (POST, GET וכו')
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// קוד תגובה HTTP
    /// </summary>
    public int ResponseStatusCode { get; set; }

    /// <summary>
    /// גוף הבקשה (מקוצר לצורך תיעוד)
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// גוף התגובה (מקוצר לצורך תיעוד)
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// פרטי שגיאה במקרה של כשלון
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// האם הקריאה הצליחה
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// כתובת IP של הקורא
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// משך הטיפול בבקשה (מילישניות)
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// חותמת זמן של הקריאה
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
