namespace CandidacyManagement.Application.Notifications;

/// <summary>
/// שירות שליחת הודעות לקבוצת מועמדויות
/// </summary>
public interface IBulkNotificationService
{
    /// <summary>
    /// שליחת הודעה לקבוצת מועמדויות
    /// </summary>
    Task<BulkNotificationResultDto> SendBulkAsync(BulkSendNotificationCommand command, CancellationToken cancellationToken = default);
}
