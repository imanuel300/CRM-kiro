using System.Linq.Expressions;
using CandidacyManagement.Application.Notifications;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Notifications;

public class NotificationServiceTests
{
    private readonly Mock<IRepository<NotificationTemplate>> _templateRepoMock;
    private readonly Mock<IRepository<NotificationLog>> _logRepoMock;
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ISmsService> _smsServiceMock;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _templateRepoMock = new Mock<IRepository<NotificationTemplate>>();
        _logRepoMock = new Mock<IRepository<NotificationLog>>();
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _emailServiceMock = new Mock<IEmailService>();
        _smsServiceMock = new Mock<ISmsService>();

        // Default: delivery always succeeds
        _emailServiceMock.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationDeliveryResult(true));
        _smsServiceMock.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationDeliveryResult(true));

        _sut = new NotificationService(
            _templateRepoMock.Object,
            _logRepoMock.Object,
            _candidacyRepoMock.Object,
            _contactRepoMock.Object,
            _orgUnitRepoMock.Object,
            _emailServiceMock.Object,
            _smsServiceMock.Object);
    }

    #region CreateTemplateAsync

    [Fact]
    public async Task CreateTemplateAsync_WithValidCommand_ReturnsTemplateDto()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _templateRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationTemplate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationTemplate e, CancellationToken _) => { e.Id = 10; return e; });

        var command = new CreateNotificationTemplateCommand(
            OrgUnitId: 1,
            Name: "זימון לראיון",
            Subject: "זימון לראיון - {{CandidateName}}",
            Body: "שלום {{CandidateName}}, הנך מוזמן/ת לראיון בתאריך {{Date}}.",
            Channel: NotificationChannel.Email,
            TriggerEvent: TriggerEventType.InterviewScheduled);

        var result = await _sut.CreateTemplateAsync(command);

        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.OrgUnitId.Should().Be(1);
        result.Name.Should().Be("זימון לראיון");
        result.Channel.Should().Be(NotificationChannel.Email);
        result.TriggerEvent.Should().Be(TriggerEventType.InterviewScheduled);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateTemplateAsync_OrgUnitNotFound_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var command = new CreateNotificationTemplateCommand(999, "Test", "Sub", "Body",
            NotificationChannel.Email, TriggerEventType.StatusChange);

        var act = () => _sut.CreateTemplateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateTemplateAsync_EmptyName_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });

        var command = new CreateNotificationTemplateCommand(1, "", "Sub", "Body",
            NotificationChannel.Email, TriggerEventType.StatusChange);

        var act = () => _sut.CreateTemplateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region UpdateTemplateAsync

    [Fact]
    public async Task UpdateTemplateAsync_WithValidCommand_ReturnsUpdatedDto()
    {
        var existing = new NotificationTemplate
        {
            Id = 10, OrgUnitId = 1, Name = "Old", Subject = "Old Sub",
            Body = "Old Body", Channel = NotificationChannel.Email,
            TriggerEvent = TriggerEventType.StatusChange, IsActive = true
        };
        _templateRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var command = new UpdateNotificationTemplateCommand(10, "Updated", "New Sub", "New Body",
            NotificationChannel.Sms, TriggerEventType.CommitteeDecision, false);

        var result = await _sut.UpdateTemplateAsync(command);

        result.Name.Should().Be("Updated");
        result.Channel.Should().Be(NotificationChannel.Sms);
        result.TriggerEvent.Should().Be(TriggerEventType.CommitteeDecision);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTemplateAsync_NotFound_ThrowsNotFoundException()
    {
        _templateRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationTemplate?)null);

        var command = new UpdateNotificationTemplateCommand(999, "Name", "Sub", "Body",
            NotificationChannel.Email, TriggerEventType.StatusChange, true);

        var act = () => _sut.UpdateTemplateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region SendManualAsync

    [Fact]
    public async Task SendManualAsync_WithValidEmail_ReturnsLogDto()
    {
        var candidacy = new Candidacy { Id = 5, ContactId = 20, OrgUnitId = 1 };
        var contact = new Contact { Id = 20, FirstName = "ישראל", LastName = "ישראלי", Email = "test@example.com" };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationLog e, CancellationToken _) => { e.Id = 100; return e; });

        var command = new SendNotificationCommand(5, "נושא", "גוף הודעה", NotificationChannel.Email);

        var result = await _sut.SendManualAsync(command);

        result.Should().NotBeNull();
        result.CandidacyId.Should().Be(5);
        result.Recipient.Should().Be("test@example.com");
        result.Status.Should().Be(NotificationStatus.Sent);
    }

    [Fact]
    public async Task SendManualAsync_NoEmail_ThrowsValidationException()
    {
        var candidacy = new Candidacy { Id = 5, ContactId = 20, OrgUnitId = 1 };
        var contact = new Contact { Id = 20, FirstName = "ישראל", LastName = "ישראלי", Email = null };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var command = new SendNotificationCommand(5, "נושא", "גוף", NotificationChannel.Email);

        var act = () => _sut.SendManualAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SendManualAsync_CandidacyNotFound_ThrowsNotFoundException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var command = new SendNotificationCommand(999, "נושא", "גוף", NotificationChannel.Email);

        var act = () => _sut.SendManualAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SendManualAsync_EmailDeliveryFails_LogsFailureStatus()
    {
        var candidacy = new Candidacy { Id = 5, ContactId = 20, OrgUnitId = 1 };
        var contact = new Contact { Id = 20, FirstName = "ישראל", LastName = "ישראלי", Email = "test@example.com" };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        _emailServiceMock.Setup(e => e.SendAsync("test@example.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationDeliveryResult(false, "SMTP connection refused"));
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationLog e, CancellationToken _) => { e.Id = 100; return e; });

        var command = new SendNotificationCommand(5, "נושא", "גוף הודעה", NotificationChannel.Email);

        var result = await _sut.SendManualAsync(command);

        result.Status.Should().Be(NotificationStatus.Failed);
        result.ErrorMessage.Should().Be("SMTP connection refused");
    }

    [Fact]
    public async Task SendManualAsync_SmsChannel_CallsSmsService()
    {
        var candidacy = new Candidacy { Id = 5, ContactId = 20, OrgUnitId = 1 };
        var contact = new Contact { Id = 20, FirstName = "ישראל", LastName = "ישראלי", Phone = "0501234567" };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationLog e, CancellationToken _) => { e.Id = 100; return e; });

        var command = new SendNotificationCommand(5, "נושא", "גוף הודעה", NotificationChannel.Sms);

        var result = await _sut.SendManualAsync(command);

        result.Status.Should().Be(NotificationStatus.Sent);
        result.Channel.Should().Be(NotificationChannel.Sms);
        _smsServiceMock.Verify(s => s.SendAsync("0501234567", "גוף הודעה", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region TriggerAsync

    [Fact]
    public async Task TriggerAsync_WithMatchingTemplate_CreatesNotificationLog()
    {
        var template = new NotificationTemplate
        {
            Id = 10, OrgUnitId = 1, Name = "שינוי סטטוס",
            Subject = "עדכון סטטוס - {{CandidateName}}",
            Body = "שלום {{CandidateName}}, הסטטוס שלך עודכן ל-{{Status}} בתאריך {{Date}}.",
            Channel = NotificationChannel.Email,
            TriggerEvent = TriggerEventType.StatusChange,
            IsActive = true
        };

        _templateRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<NotificationTemplate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationTemplate> { template });

        var candidacy = new Candidacy { Id = 5, ContactId = 20, OrgUnitId = 1 };
        var contact = new Contact { Id = 20, FirstName = "ישראל", LastName = "ישראלי", Email = "test@example.com" };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationLog e, CancellationToken _) => { e.Id = 200; return e; });

        var variables = new Dictionary<string, string> { ["Status"] = "עבר ראיון" };

        await _sut.TriggerAsync(1, 5, TriggerEventType.StatusChange, variables);

        _logRepoMock.Verify(r => r.AddAsync(It.Is<NotificationLog>(l =>
            l.CandidacyId == 5 &&
            l.TemplateId == 10 &&
            l.Status == NotificationStatus.Sent &&
            l.Subject.Contains("ישראל ישראלי") &&
            l.Body.Contains("עבר ראיון")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TriggerAsync_NoMatchingTemplates_DoesNotCreateLog()
    {
        _templateRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<NotificationTemplate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationTemplate>());

        await _sut.TriggerAsync(1, 5, TriggerEventType.StatusChange, new Dictionary<string, string>());

        _logRepoMock.Verify(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TriggerAsync_ContactWithoutEmail_LogsFailure()
    {
        var template = new NotificationTemplate
        {
            Id = 10, OrgUnitId = 1, Name = "Test",
            Subject = "Test", Body = "Test",
            Channel = NotificationChannel.Email,
            TriggerEvent = TriggerEventType.StatusChange,
            IsActive = true
        };

        _templateRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<NotificationTemplate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationTemplate> { template });

        var candidacy = new Candidacy { Id = 5, ContactId = 20, OrgUnitId = 1 };
        var contact = new Contact { Id = 20, FirstName = "ישראל", LastName = "ישראלי", Email = null };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationLog e, CancellationToken _) => { e.Id = 300; return e; });

        await _sut.TriggerAsync(1, 5, TriggerEventType.StatusChange, new Dictionary<string, string>());

        _logRepoMock.Verify(r => r.AddAsync(It.Is<NotificationLog>(l =>
            l.Status == NotificationStatus.Failed &&
            l.ErrorMessage != null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TriggerAsync_MultipleTemplatesForSameEvent_SendsAllNotifications()
    {
        var template1 = new NotificationTemplate
        {
            Id = 10, OrgUnitId = 1, Name = "הודעת דוא\"ל",
            Subject = "עדכון - {{CandidateName}}", Body = "גוף הודעה 1",
            Channel = NotificationChannel.Email,
            TriggerEvent = TriggerEventType.StatusChange, IsActive = true
        };
        var template2 = new NotificationTemplate
        {
            Id = 11, OrgUnitId = 1, Name = "הודעת SMS",
            Subject = "עדכון SMS", Body = "גוף הודעה 2",
            Channel = NotificationChannel.Sms,
            TriggerEvent = TriggerEventType.StatusChange, IsActive = true
        };

        _templateRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<NotificationTemplate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationTemplate> { template1, template2 });

        var candidacy = new Candidacy { Id = 5, ContactId = 20, OrgUnitId = 1 };
        var contact = new Contact { Id = 20, FirstName = "דנה", LastName = "כהן", Email = "dana@test.com", Phone = "0501234567" };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationLog e, CancellationToken _) => { e.Id = new Random().Next(1, 10000); return e; });

        await _sut.TriggerAsync(1, 5, TriggerEventType.StatusChange, new Dictionary<string, string>());

        _logRepoMock.Verify(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _emailServiceMock.Verify(e => e.SendAsync("dana@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _smsServiceMock.Verify(s => s.SendAsync("0501234567", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TriggerAsync_DeliveryFails_LogsFailureWithErrorMessage()
    {
        var template = new NotificationTemplate
        {
            Id = 10, OrgUnitId = 1, Name = "שינוי סטטוס",
            Subject = "עדכון", Body = "גוף",
            Channel = NotificationChannel.Email,
            TriggerEvent = TriggerEventType.StatusChange, IsActive = true
        };

        _templateRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<NotificationTemplate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationTemplate> { template });

        var candidacy = new Candidacy { Id = 5, ContactId = 20, OrgUnitId = 1 };
        var contact = new Contact { Id = 20, FirstName = "ישראל", LastName = "ישראלי", Email = "test@example.com" };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        _emailServiceMock.Setup(e => e.SendAsync("test@example.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationDeliveryResult(false, "שגיאת SMTP: שרת לא זמין"));
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationLog e, CancellationToken _) => { e.Id = 400; return e; });

        await _sut.TriggerAsync(1, 5, TriggerEventType.StatusChange, new Dictionary<string, string>());

        _logRepoMock.Verify(r => r.AddAsync(It.Is<NotificationLog>(l =>
            l.Status == NotificationStatus.Failed &&
            l.ErrorMessage == "שגיאת SMTP: שרת לא זמין"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TriggerAsync_EmailServiceThrowsException_LogsFailure()
    {
        var template = new NotificationTemplate
        {
            Id = 10, OrgUnitId = 1, Name = "Test",
            Subject = "Test", Body = "Test",
            Channel = NotificationChannel.Email,
            TriggerEvent = TriggerEventType.StatusChange, IsActive = true
        };

        _templateRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<NotificationTemplate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationTemplate> { template });

        var candidacy = new Candidacy { Id = 5, ContactId = 20, OrgUnitId = 1 };
        var contact = new Contact { Id = 20, FirstName = "ישראל", LastName = "ישראלי", Email = "test@example.com" };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        _emailServiceMock.Setup(e => e.SendAsync("test@example.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection timeout"));
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationLog e, CancellationToken _) => { e.Id = 500; return e; });

        await _sut.TriggerAsync(1, 5, TriggerEventType.StatusChange, new Dictionary<string, string>());

        _logRepoMock.Verify(r => r.AddAsync(It.Is<NotificationLog>(l =>
            l.Status == NotificationStatus.Failed &&
            l.ErrorMessage!.Contains("Connection timeout")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region RenderTemplate

    [Fact]
    public void RenderTemplate_ReplacesVariables()
    {
        var template = "שלום {{CandidateName}}, הסטטוס שלך: {{Status}}";
        var variables = new Dictionary<string, string>
        {
            ["CandidateName"] = "ישראל ישראלי",
            ["Status"] = "התקבל"
        };

        var result = NotificationService.RenderTemplate(template, variables);

        result.Should().Be("שלום ישראל ישראלי, הסטטוס שלך: התקבל");
    }

    [Fact]
    public void RenderTemplate_UnknownVariable_KeepsPlaceholder()
    {
        var template = "שלום {{CandidateName}}, {{Unknown}}";
        var variables = new Dictionary<string, string>
        {
            ["CandidateName"] = "ישראל"
        };

        var result = NotificationService.RenderTemplate(template, variables);

        result.Should().Be("שלום ישראל, {{Unknown}}");
    }

    [Fact]
    public void RenderTemplate_EmptyTemplate_ReturnsEmpty()
    {
        var result = NotificationService.RenderTemplate("", new Dictionary<string, string>());
        result.Should().BeEmpty();
    }

    [Fact]
    public void RenderTemplate_AllVariableTypes_ReplacesCorrectly()
    {
        var template = "שלום {{CandidateName}}, סטטוס: {{Status}}, תאריך: {{Date}}";
        var variables = new Dictionary<string, string>
        {
            ["CandidateName"] = "דנה כהן",
            ["Status"] = "עבר ראיון",
            ["Date"] = "15/01/2025"
        };

        var result = NotificationService.RenderTemplate(template, variables);

        result.Should().Be("שלום דנה כהן, סטטוס: עבר ראיון, תאריך: 15/01/2025");
    }

    [Fact]
    public void RenderTemplate_NullTemplate_ReturnsNull()
    {
        var result = NotificationService.RenderTemplate(null!, new Dictionary<string, string>());
        result.Should().BeNull();
    }

    [Fact]
    public void RenderTemplate_NoVariablesInTemplate_ReturnsOriginal()
    {
        var template = "הודעה ללא משתנים דינמיים";
        var variables = new Dictionary<string, string> { ["CandidateName"] = "ישראל" };

        var result = NotificationService.RenderTemplate(template, variables);

        result.Should().Be("הודעה ללא משתנים דינמיים");
    }

    [Fact]
    public void RenderTemplate_EmptyVariablesDictionary_KeepsAllPlaceholders()
    {
        var template = "שלום {{CandidateName}}, תאריך: {{Date}}";
        var variables = new Dictionary<string, string>();

        var result = NotificationService.RenderTemplate(template, variables);

        result.Should().Be("שלום {{CandidateName}}, תאריך: {{Date}}");
    }

    #endregion

    #region GetLogsAsync

    [Fact]
    public async Task GetLogsAsync_ByCandidacyId_ReturnsFilteredLogs()
    {
        var logs = new List<NotificationLog>
        {
            new() { Id = 1, CandidacyId = 5, Channel = NotificationChannel.Email,
                     Recipient = "a@b.com", Subject = "Test", Body = "Body",
                     Status = NotificationStatus.Sent, SentAt = DateTime.UtcNow }
        };

        _logRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<NotificationLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        var result = await _sut.GetLogsAsync(new NotificationLogQueryParams(CandidacyId: 5));

        result.Should().HaveCount(1);
        result.First().CandidacyId.Should().Be(5);
    }

    #endregion

    #region DeleteTemplateAsync

    [Fact]
    public async Task DeleteTemplateAsync_ExistingTemplate_DeletesSuccessfully()
    {
        var template = new NotificationTemplate { Id = 10, OrgUnitId = 1, Name = "Test" };
        _templateRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        await _sut.DeleteTemplateAsync(10);

        _templateRepoMock.Verify(r => r.DeleteAsync(template, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTemplateAsync_NotFound_ThrowsNotFoundException()
    {
        _templateRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationTemplate?)null);

        var act = () => _sut.DeleteTemplateAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
