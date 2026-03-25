using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.Notifications;

public record NotificationTemplateDto(
    int Id,
    int OrgUnitId,
    string Name,
    string Subject,
    string Body,
    NotificationChannel Channel,
    TriggerEventType TriggerEvent,
    bool IsActive,
    DateTime CreatedAt);

public record NotificationLogDto(
    int Id,
    int CandidacyId,
    int? TemplateId,
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    string Body,
    NotificationStatus Status,
    string? ErrorMessage,
    DateTime SentAt);

public record CreateNotificationTemplateCommand(
    int OrgUnitId,
    string Name,
    string Subject,
    string Body,
    NotificationChannel Channel,
    TriggerEventType TriggerEvent);

public record UpdateNotificationTemplateCommand(
    int Id,
    string Name,
    string Subject,
    string Body,
    NotificationChannel Channel,
    TriggerEventType TriggerEvent,
    bool IsActive);

public record SendNotificationCommand(
    int CandidacyId,
    string Subject,
    string Body,
    NotificationChannel Channel);

public record NotificationTemplateQueryParams(
    int? OrgUnitId = null,
    TriggerEventType? TriggerEvent = null);

public record NotificationLogQueryParams(
    int? CandidacyId = null,
    int? OrgUnitId = null);

/// <summary>
/// פקודת שליחת הודעה לקבוצת מועמדויות
/// </summary>
public record BulkSendNotificationCommand(
    IReadOnlyList<int> CandidacyIds,
    string Subject,
    string Body,
    NotificationChannel Channel);

/// <summary>
/// תוצאת שליחה בודדת (Email/SMS)
/// </summary>
public record NotificationDeliveryResult(
    bool Success,
    string? ErrorMessage = null);

/// <summary>
/// תוצאת שליחה מרובה
/// </summary>
public record BulkNotificationResultDto(
    int TotalRequested,
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<NotificationLogDto> Results);
