using System.Linq.Expressions;
using CandidacyManagement.Application.Notifications;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Notifications;

public class BulkNotificationServiceTests
{
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<NotificationLog>> _logRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ISmsService> _smsServiceMock;
    private readonly BulkNotificationService _sut;

    public BulkNotificationServiceTests()
    {
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _logRepoMock = new Mock<IRepository<NotificationLog>>();
        _emailServiceMock = new Mock<IEmailService>();
        _smsServiceMock = new Mock<ISmsService>();

        _emailServiceMock.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationDeliveryResult(true));
        _smsServiceMock.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationDeliveryResult(true));

        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationLog e, CancellationToken _) => { e.Id = new Random().Next(1, 10000); return e; });

        _sut = new BulkNotificationService(
            _candidacyRepoMock.Object,
            _contactRepoMock.Object,
            _logRepoMock.Object,
            _emailServiceMock.Object,
            _smsServiceMock.Object);
    }

    [Fact]
    public async Task SendBulkAsync_EmptyCandidacyIds_ThrowsValidationException()
    {
        var command = new BulkSendNotificationCommand(
            new List<int>(), "נושא", "גוף", NotificationChannel.Email);

        var act = () => _sut.SendBulkAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SendBulkAsync_AllSuccessful_ReturnsCorrectCounts()
    {
        SetupCandidacyAndContact(1, 10, "a@b.com", "050-1111111");
        SetupCandidacyAndContact(2, 20, "c@d.com", "050-2222222");

        var command = new BulkSendNotificationCommand(
            new List<int> { 1, 2 }, "נושא", "גוף", NotificationChannel.Email);

        var result = await _sut.SendBulkAsync(command);

        result.TotalRequested.Should().Be(2);
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(0);
        result.Results.Should().HaveCount(2);
        result.Results.Should().AllSatisfy(r => r.Status.Should().Be(NotificationStatus.Sent));
    }

    [Fact]
    public async Task SendBulkAsync_CandidacyNotFound_LogsFailure()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var command = new BulkSendNotificationCommand(
            new List<int> { 999 }, "נושא", "גוף", NotificationChannel.Email);

        var result = await _sut.SendBulkAsync(command);

        result.FailureCount.Should().Be(1);
        result.Results.First().Status.Should().Be(NotificationStatus.Failed);
        result.Results.First().ErrorMessage.Should().Contain("999");
    }

    [Fact]
    public async Task SendBulkAsync_ContactWithoutEmail_LogsFailure()
    {
        SetupCandidacyAndContact(1, 10, email: null, phone: "050-1111111");

        var command = new BulkSendNotificationCommand(
            new List<int> { 1 }, "נושא", "גוף", NotificationChannel.Email);

        var result = await _sut.SendBulkAsync(command);

        result.FailureCount.Should().Be(1);
        result.Results.First().Status.Should().Be(NotificationStatus.Failed);
    }

    [Fact]
    public async Task SendBulkAsync_MixedResults_ReturnsCorrectCounts()
    {
        SetupCandidacyAndContact(1, 10, "a@b.com", "050-1111111");
        SetupCandidacyAndContact(2, 20, email: null, phone: null);

        var command = new BulkSendNotificationCommand(
            new List<int> { 1, 2 }, "נושא", "גוף", NotificationChannel.Email);

        var result = await _sut.SendBulkAsync(command);

        result.TotalRequested.Should().Be(2);
        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(1);
    }

    [Fact]
    public async Task SendBulkAsync_EmailDeliveryFails_LogsFailureWithError()
    {
        SetupCandidacyAndContact(1, 10, "a@b.com", "050-1111111");
        _emailServiceMock.Setup(e => e.SendAsync("a@b.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationDeliveryResult(false, "SMTP error"));

        var command = new BulkSendNotificationCommand(
            new List<int> { 1 }, "נושא", "גוף", NotificationChannel.Email);

        var result = await _sut.SendBulkAsync(command);

        result.FailureCount.Should().Be(1);
        result.Results.First().Status.Should().Be(NotificationStatus.Failed);
        result.Results.First().ErrorMessage.Should().Be("SMTP error");
    }

    [Fact]
    public async Task SendBulkAsync_SmsChannel_CallsSmsService()
    {
        SetupCandidacyAndContact(1, 10, "a@b.com", "050-1111111");

        var command = new BulkSendNotificationCommand(
            new List<int> { 1 }, "נושא", "גוף SMS", NotificationChannel.Sms);

        var result = await _sut.SendBulkAsync(command);

        result.SuccessCount.Should().Be(1);
        _smsServiceMock.Verify(s => s.SendAsync("050-1111111", "גוף SMS", It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupCandidacyAndContact(int candidacyId, int contactId, string? email, string? phone)
    {
        var candidacy = new Candidacy { Id = candidacyId, ContactId = contactId, OrgUnitId = 1 };
        var contact = new Contact { Id = contactId, FirstName = "Test", LastName = "User", Email = email, Phone = phone };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(candidacyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
    }
}
