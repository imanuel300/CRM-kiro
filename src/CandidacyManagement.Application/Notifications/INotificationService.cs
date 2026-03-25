namespace CandidacyManagement.Application.Notifications;

public interface INotificationService
{
    // ניהול תבניות
    Task<NotificationTemplateDto> CreateTemplateAsync(CreateNotificationTemplateCommand command, CancellationToken cancellationToken = default);
    Task<NotificationTemplateDto> UpdateTemplateAsync(UpdateNotificationTemplateCommand command, CancellationToken cancellationToken = default);
    Task<NotificationTemplateDto> GetTemplateByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationTemplateDto>> ListTemplatesAsync(NotificationTemplateQueryParams query, CancellationToken cancellationToken = default);
    Task DeleteTemplateAsync(int id, CancellationToken cancellationToken = default);

    // שליחה ידנית
    Task<NotificationLogDto> SendManualAsync(SendNotificationCommand command, CancellationToken cancellationToken = default);

    // שליחה אוטומטית בעקבות אירוע מפעיל
    Task TriggerAsync(int orgUnitId, int candidacyId, Domain.Enums.TriggerEventType triggerEvent, Dictionary<string, string> variables, CancellationToken cancellationToken = default);

    // יומן הודעות
    Task<IEnumerable<NotificationLogDto>> GetLogsAsync(NotificationLogQueryParams query, CancellationToken cancellationToken = default);
}
