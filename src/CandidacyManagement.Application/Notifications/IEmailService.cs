namespace CandidacyManagement.Application.Notifications;

/// <summary>
/// שירות שליחת דוא"ל - ממשק לשליחת הודעות בדוא"ל
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// שליחת הודעת דוא"ל
    /// </summary>
    /// <param name="to">כתובת דוא"ל של הנמען</param>
    /// <param name="subject">נושא ההודעה</param>
    /// <param name="body">גוף ההודעה</param>
    /// <param name="cancellationToken">טוקן ביטול</param>
    /// <returns>תוצאת השליחה - הצלחה או כשלון עם הודעת שגיאה</returns>
    Task<NotificationDeliveryResult> SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
