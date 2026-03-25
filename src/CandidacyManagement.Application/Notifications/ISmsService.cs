namespace CandidacyManagement.Application.Notifications;

/// <summary>
/// שירות שליחת SMS - ממשק לשליחת הודעות טקסט
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// שליחת הודעת SMS
    /// </summary>
    /// <param name="phoneNumber">מספר טלפון של הנמען</param>
    /// <param name="message">תוכן ההודעה</param>
    /// <param name="cancellationToken">טוקן ביטול</param>
    /// <returns>תוצאת השליחה - הצלחה או כשלון עם הודעת שגיאה</returns>
    Task<NotificationDeliveryResult> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
