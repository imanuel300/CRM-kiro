using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Notifications;

/// <summary>
/// שירות שליחת הודעות לקבוצת מועמדויות
/// </summary>
public class BulkNotificationService : IBulkNotificationService
{
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<NotificationLog> _logRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public BulkNotificationService(
        IRepository<Candidacy> candidacyRepository,
        IRepository<Contact> contactRepository,
        IRepository<NotificationLog> logRepository,
        IEmailService emailService,
        ISmsService smsService)
    {
        _candidacyRepository = candidacyRepository;
        _contactRepository = contactRepository;
        _logRepository = logRepository;
        _emailService = emailService;
        _smsService = smsService;
    }

    public async Task<BulkNotificationResultDto> SendBulkAsync(BulkSendNotificationCommand command, CancellationToken cancellationToken = default)
    {
        if (command.CandidacyIds == null || command.CandidacyIds.Count == 0)
            throw new ValidationException("CandidacyIds", "יש לציין לפחות מועמדות אחת לשליחה");

        if (string.IsNullOrWhiteSpace(command.Subject) && command.Channel == NotificationChannel.Email)
            throw new ValidationException("Subject", "נושא ההודעה הוא שדה חובה בדוא\"ל");

        if (string.IsNullOrWhiteSpace(command.Body))
            throw new ValidationException("Body", "גוף ההודעה הוא שדה חובה");

        var results = new List<NotificationLogDto>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var candidacyId in command.CandidacyIds)
        {
            var log = await SendToCandidacyAsync(candidacyId, command, cancellationToken);
            results.Add(log);

            if (log.Status == NotificationStatus.Sent)
                successCount++;
            else
                failureCount++;
        }

        return new BulkNotificationResultDto(
            TotalRequested: command.CandidacyIds.Count,
            SuccessCount: successCount,
            FailureCount: failureCount,
            Results: results);
    }

    private async Task<NotificationLogDto> SendToCandidacyAsync(
        int candidacyId,
        BulkSendNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken);
        if (candidacy == null)
        {
            return await LogFailureAsync(candidacyId, command, $"מועמדות {candidacyId} לא נמצאה", cancellationToken);
        }

        var contact = await _contactRepository.GetByIdAsync(candidacy.ContactId, cancellationToken);
        if (contact == null)
        {
            return await LogFailureAsync(candidacyId, command, $"איש קשר {candidacy.ContactId} לא נמצא", cancellationToken);
        }

        var recipient = command.Channel == NotificationChannel.Email
            ? contact.Email ?? string.Empty
            : contact.Phone ?? string.Empty;

        if (string.IsNullOrWhiteSpace(recipient))
        {
            var channelName = command.Channel == NotificationChannel.Email ? "כתובת דוא\"ל" : "מספר טלפון";
            return await LogFailureAsync(candidacyId, command, $"לאיש הקשר אין {channelName}", cancellationToken);
        }

        // שליחה בפועל דרך שירות הדוא"ל או ה-SMS
        NotificationDeliveryResult deliveryResult;
        try
        {
            deliveryResult = command.Channel == NotificationChannel.Email
                ? await _emailService.SendAsync(recipient, command.Subject, command.Body, cancellationToken)
                : await _smsService.SendAsync(recipient, command.Body, cancellationToken);
        }
        catch (Exception ex)
        {
            deliveryResult = new NotificationDeliveryResult(false, ex.Message);
        }

        var status = deliveryResult.Success ? NotificationStatus.Sent : NotificationStatus.Failed;

        var log = new NotificationLog
        {
            CandidacyId = candidacyId,
            Channel = command.Channel,
            Recipient = recipient,
            Subject = command.Subject,
            Body = command.Body,
            Status = status,
            ErrorMessage = deliveryResult.ErrorMessage,
            SentAt = DateTime.UtcNow
        };

        await _logRepository.AddAsync(log, cancellationToken);

        return ToLogDto(log);
    }

    private async Task<NotificationLogDto> LogFailureAsync(
        int candidacyId,
        BulkSendNotificationCommand command,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var log = new NotificationLog
        {
            CandidacyId = candidacyId,
            Channel = command.Channel,
            Recipient = string.Empty,
            Subject = command.Subject,
            Body = command.Body,
            Status = NotificationStatus.Failed,
            ErrorMessage = errorMessage,
            SentAt = DateTime.UtcNow
        };

        await _logRepository.AddAsync(log, cancellationToken);
        return ToLogDto(log);
    }

    private static NotificationLogDto ToLogDto(NotificationLog entity) =>
        new(entity.Id, entity.CandidacyId, entity.TemplateId, entity.Channel,
            entity.Recipient, entity.Subject, entity.Body, entity.Status,
            entity.ErrorMessage, entity.SentAt);
}
