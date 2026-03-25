using System.Text.RegularExpressions;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Notifications;

public class NotificationService : INotificationService
{
    private readonly IRepository<NotificationTemplate> _templateRepository;
    private readonly IRepository<NotificationLog> _logRepository;
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public NotificationService(
        IRepository<NotificationTemplate> templateRepository,
        IRepository<NotificationLog> logRepository,
        IRepository<Candidacy> candidacyRepository,
        IRepository<Contact> contactRepository,
        IRepository<OrganizationalUnit> orgUnitRepository,
        IEmailService emailService,
        ISmsService smsService)
    {
        _templateRepository = templateRepository;
        _logRepository = logRepository;
        _candidacyRepository = candidacyRepository;
        _contactRepository = contactRepository;
        _orgUnitRepository = orgUnitRepository;
        _emailService = emailService;
        _smsService = smsService;
    }

    // --- ניהול תבניות ---

    public async Task<NotificationTemplateDto> CreateTemplateAsync(CreateNotificationTemplateCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "שם תבנית הוא שדה חובה");

        if (string.IsNullOrWhiteSpace(command.Subject))
            throw new ValidationException("Subject", "נושא ההודעה הוא שדה חובה");

        if (string.IsNullOrWhiteSpace(command.Body))
            throw new ValidationException("Body", "גוף ההודעה הוא שדה חובה");

        var entity = new NotificationTemplate
        {
            OrgUnitId = command.OrgUnitId,
            Name = command.Name,
            Subject = command.Subject,
            Body = command.Body,
            Channel = command.Channel,
            TriggerEvent = command.TriggerEvent,
            IsActive = true
        };

        await _templateRepository.AddAsync(entity, cancellationToken);
        return ToTemplateDto(entity);
    }

    public async Task<NotificationTemplateDto> UpdateTemplateAsync(UpdateNotificationTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _templateRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(NotificationTemplate), command.Id);

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "שם תבנית הוא שדה חובה");

        entity.Name = command.Name;
        entity.Subject = command.Subject;
        entity.Body = command.Body;
        entity.Channel = command.Channel;
        entity.TriggerEvent = command.TriggerEvent;
        entity.IsActive = command.IsActive;

        await _templateRepository.UpdateAsync(entity, cancellationToken);
        return ToTemplateDto(entity);
    }

    public async Task<NotificationTemplateDto> GetTemplateByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _templateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(NotificationTemplate), id);

        return ToTemplateDto(entity);
    }

    public async Task<IEnumerable<NotificationTemplateDto>> ListTemplatesAsync(NotificationTemplateQueryParams query, CancellationToken cancellationToken = default)
    {
        var results = await _templateRepository.FindAsync(t =>
            (!query.OrgUnitId.HasValue || t.OrgUnitId == query.OrgUnitId.Value) &&
            (!query.TriggerEvent.HasValue || t.TriggerEvent == query.TriggerEvent.Value),
            cancellationToken);

        return results.Select(ToTemplateDto);
    }

    public async Task DeleteTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _templateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(NotificationTemplate), id);

        await _templateRepository.DeleteAsync(entity, cancellationToken);
    }

    // --- שליחה ידנית ---

    public async Task<NotificationLogDto> SendManualAsync(SendNotificationCommand command, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        var contact = await _contactRepository.GetByIdAsync(candidacy.ContactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), candidacy.ContactId);

        var recipient = command.Channel == NotificationChannel.Email
            ? contact.Email ?? string.Empty
            : contact.Phone ?? string.Empty;

        if (string.IsNullOrWhiteSpace(recipient))
            throw new ValidationException("Recipient", $"לאיש הקשר אין {(command.Channel == NotificationChannel.Email ? "כתובת דוא\"ל" : "מספר טלפון")}");

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
            CandidacyId = command.CandidacyId,
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

    // --- שליחה אוטומטית בעקבות אירוע מפעיל ---

    public async Task TriggerAsync(int orgUnitId, int candidacyId, TriggerEventType triggerEvent, Dictionary<string, string> variables, CancellationToken cancellationToken = default)
    {
        // מציאת תבניות פעילות המקושרות לאירוע המפעיל
        var templates = await _templateRepository.FindAsync(
            t => t.OrgUnitId == orgUnitId && t.TriggerEvent == triggerEvent && t.IsActive,
            cancellationToken);

        if (!templates.Any())
            return;

        var candidacy = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken);
        if (candidacy == null)
            return;

        var contact = await _contactRepository.GetByIdAsync(candidacy.ContactId, cancellationToken);
        if (contact == null)
            return;

        // הוספת משתנים דינמיים בסיסיים
        var allVariables = new Dictionary<string, string>(variables)
        {
            ["CandidateName"] = $"{contact.FirstName} {contact.LastName}",
            ["Date"] = DateTime.UtcNow.ToString("dd/MM/yyyy")
        };

        foreach (var template in templates)
        {
            var recipient = template.Channel == NotificationChannel.Email
                ? contact.Email ?? string.Empty
                : contact.Phone ?? string.Empty;

            var renderedSubject = RenderTemplate(template.Subject, allVariables);
            var renderedBody = RenderTemplate(template.Body, allVariables);

            NotificationStatus status;
            string? errorMessage;

            if (string.IsNullOrWhiteSpace(recipient))
            {
                status = NotificationStatus.Failed;
                errorMessage = "לנמען אין פרטי קשר מתאימים לערוץ השליחה";
            }
            else
            {
                // שליחה בפועל דרך שירות הדוא"ל או ה-SMS
                NotificationDeliveryResult deliveryResult;
                try
                {
                    deliveryResult = template.Channel == NotificationChannel.Email
                        ? await _emailService.SendAsync(recipient, renderedSubject, renderedBody, cancellationToken)
                        : await _smsService.SendAsync(recipient, renderedBody, cancellationToken);
                }
                catch (Exception ex)
                {
                    deliveryResult = new NotificationDeliveryResult(false, ex.Message);
                }

                status = deliveryResult.Success ? NotificationStatus.Sent : NotificationStatus.Failed;
                errorMessage = deliveryResult.ErrorMessage;
            }

            var log = new NotificationLog
            {
                CandidacyId = candidacyId,
                TemplateId = template.Id,
                Channel = template.Channel,
                Recipient = recipient,
                Subject = renderedSubject,
                Body = renderedBody,
                Status = status,
                ErrorMessage = errorMessage,
                SentAt = DateTime.UtcNow
            };

            await _logRepository.AddAsync(log, cancellationToken);
        }
    }

    // --- יומן הודעות ---

    public async Task<IEnumerable<NotificationLogDto>> GetLogsAsync(NotificationLogQueryParams query, CancellationToken cancellationToken = default)
    {
        var results = await _logRepository.FindAsync(l =>
            (!query.CandidacyId.HasValue || l.CandidacyId == query.CandidacyId.Value),
            cancellationToken);

        return results.Select(ToLogDto);
    }

    // --- עזר ---

    /// <summary>
    /// החלפת משתנים דינמיים בתבנית: {{CandidateName}}, {{Status}}, {{Date}}
    /// </summary>
    internal static string RenderTemplate(string template, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        return Regex.Replace(template, @"\{\{(\w+)\}\}", match =>
        {
            var key = match.Groups[1].Value;
            return variables.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    private static NotificationTemplateDto ToTemplateDto(NotificationTemplate entity) =>
        new(entity.Id, entity.OrgUnitId, entity.Name, entity.Subject, entity.Body,
            entity.Channel, entity.TriggerEvent, entity.IsActive, entity.CreatedAt);

    private static NotificationLogDto ToLogDto(NotificationLog entity) =>
        new(entity.Id, entity.CandidacyId, entity.TemplateId, entity.Channel,
            entity.Recipient, entity.Subject, entity.Body, entity.Status,
            entity.ErrorMessage, entity.SentAt);
}
