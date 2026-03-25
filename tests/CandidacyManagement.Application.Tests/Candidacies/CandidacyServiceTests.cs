using System.Linq.Expressions;
using CandidacyManagement.Application.Candidacies;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Candidacies;

public class CandidacyServiceTests
{
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IRepository<CallForCandidates>> _callRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly Mock<IRepository<WorkflowDefinition>> _workflowRepoMock;
    private readonly Mock<IRepository<CandidacyCustomFieldValue>> _customFieldValueRepoMock;
    private readonly Mock<IRepository<CustomFieldDefinition>> _customFieldDefRepoMock;
    private readonly Mock<IRepository<StatusTransition>> _transitionRepoMock;
    private readonly Mock<IRepository<CandidacyStatusHistory>> _historyRepoMock;
    private readonly CandidacyService _sut;

    public CandidacyServiceTests()
    {
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _callRepoMock = new Mock<IRepository<CallForCandidates>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        _workflowRepoMock = new Mock<IRepository<WorkflowDefinition>>();
        _customFieldValueRepoMock = new Mock<IRepository<CandidacyCustomFieldValue>>();
        _customFieldDefRepoMock = new Mock<IRepository<CustomFieldDefinition>>();
        _transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        _historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();

        _sut = new CandidacyService(
            _candidacyRepoMock.Object,
            _contactRepoMock.Object,
            _orgUnitRepoMock.Object,
            _callRepoMock.Object,
            _statusRepoMock.Object,
            _workflowRepoMock.Object,
            _customFieldValueRepoMock.Object,
            _customFieldDefRepoMock.Object,
            _transitionRepoMock.Object,
            _historyRepoMock.Object);
    }

    #region Create

    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsDtoWithInitialStatus()
    {
        var command = new CreateCandidacyCommand(1, 1, 1);
        SetupValidCreateDependencies();

        var result = await _sut.CreateAsync(command);

        result.Should().NotBeNull();
        result.ContactId.Should().Be(1);
        result.OrgUnitId.Should().Be(1);
        result.CallForCandidatesId.Should().Be(1);
        result.CurrentStatusId.Should().Be(10);
        result.IsActive.Should().BeTrue();
        result.SubmittedAt.Should().NotBeNull();
        result.WorkflowDefinitionVersion.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateActiveCandidacy_ThrowsBusinessRuleViolation()
    {
        var command = new CreateCandidacyCommand(1, 1, 1);
        SetupValidCreateDependencies();

        _candidacyRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("*מועמדות כפולה*");
    }

    [Fact]
    public async Task CreateAsync_ClosedCallForCandidates_ThrowsBusinessRuleViolation()
    {
        var command = new CreateCandidacyCommand(1, 1, 1);

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1, IdNumber = "123456789", FirstName = "ישראל", LastName = "ישראלי" });
        _orgUnitRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1, Name = "יחידה" });
        _callRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates
            {
                Id = 1, OrgUnitId = 1, Title = "קול קורא סגור",
                CloseDate = DateTime.UtcNow.AddDays(-1)
            });

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("*סגירה*");
    }

    [Fact]
    public async Task CreateAsync_ContactNotFound_ThrowsNotFoundException()
    {
        var command = new CreateCandidacyCommand(999, 1, 1);

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_OrgUnitNotFound_ThrowsNotFoundException()
    {
        var command = new CreateCandidacyCommand(1, 999, 1);

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1, IdNumber = "123", FirstName = "א", LastName = "ב" });
        _orgUnitRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_CallForCandidatesNotFound_ThrowsNotFoundException()
    {
        var command = new CreateCandidacyCommand(1, 1, 999);

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1, IdNumber = "123", FirstName = "א", LastName = "ב" });
        _orgUnitRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1, Name = "יחידה" });
        _callRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_NoInitialStatus_SetsStatusToNull()
    {
        var command = new CreateCandidacyCommand(1, 1, 1);

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1, IdNumber = "123", FirstName = "א", LastName = "ב" });
        _orgUnitRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1, Name = "יחידה" });
        _callRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 1, OrgUnitId = 1, Title = "קול קורא" });
        _candidacyRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _statusRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<StatusDefinition>());
        _workflowRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<WorkflowDefinition>());
        _candidacyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Candidacy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy e, CancellationToken _) => e);

        var result = await _sut.CreateAsync(command);

        result.CurrentStatusId.Should().BeNull();
        result.WorkflowDefinitionVersion.Should().BeNull();
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_ExistingCandidacy_ReturnsDto()
    {
        var entity = CreateCandidacyEntity(1);

        _candidacyRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.ContactId.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _candidacyRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region List

    [Fact]
    public async Task ListAsync_WithOrgUnitFilter_ReturnsFilteredResults()
    {
        var candidacies = new List<Candidacy>
        {
            CreateCandidacyEntity(1, orgUnitId: 1),
            CreateCandidacyEntity(2, orgUnitId: 1)
        };

        _candidacyRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacies);

        var query = new CandidacyQueryParams(OrgUnitId: 1);
        var result = (await _sut.ListAsync(query)).ToList();

        result.Should().HaveCount(2);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_ExistingCandidacy_CallsDelete()
    {
        var entity = CreateCandidacyEntity(1);

        _candidacyRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _sut.DeleteAsync(1);

        _candidacyRepoMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        _candidacyRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region CustomFields

    [Fact]
    public async Task GetCustomFieldsAsync_ReturnsFieldsWithValues()
    {
        var entity = CreateCandidacyEntity(1, orgUnitId: 1);
        var definitions = new List<CustomFieldDefinition>
        {
            new() { Id = 10, OrgUnitId = 1, EntityType = "Candidacy", FieldName = "העדפת מחוז", FieldType = "Select", SortOrder = 1 },
            new() { Id = 11, OrgUnitId = 1, EntityType = "Candidacy", FieldName = "סוג תפקיד", FieldType = "Text", SortOrder = 2 }
        };
        var values = new List<CandidacyCustomFieldValue>
        {
            new() { Id = 100, CandidacyId = 1, CustomFieldDefinitionId = 10, Value = "מרכז" }
        };

        _candidacyRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _customFieldDefRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<CustomFieldDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(definitions);
        _customFieldValueRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<CandidacyCustomFieldValue, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(values);

        var result = (await _sut.GetCustomFieldsAsync(1)).ToList();

        result.Should().HaveCount(2);
        result[0].FieldName.Should().Be("העדפת מחוז");
        result[0].Value.Should().Be("מרכז");
        result[1].FieldName.Should().Be("סוג תפקיד");
        result[1].Value.Should().BeNull();
    }

    [Fact]
    public async Task SetCustomFieldValueAsync_WrongEntityType_ThrowsBusinessRuleViolation()
    {
        var entity = CreateCandidacyEntity(1, orgUnitId: 1);
        var definition = new CustomFieldDefinition { Id = 10, OrgUnitId = 1, EntityType = "Contact", FieldName = "שדה", FieldType = "Text" };

        _candidacyRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _customFieldDefRepoMock
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        var command = new SetCandidacyCustomFieldValueCommand(1, 10, "ערך");
        var act = () => _sut.SetCustomFieldValueAsync(command);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    #endregion

    #region TransitionStatus

    [Fact]
    public async Task TransitionStatusAsync_ValidTransition_UpdatesStatusAndRecordsHistory()
    {
        var candidacy = CreateCandidacyEntity(1, orgUnitId: 1);
        var command = new TransitionStatusCommand(1, 20, "עבר בדיקה", 100);

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(candidacy);
        _statusRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 10, OrgUnitId = 1, Code = "submitted", IsFinal = false });
        _statusRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 20, OrgUnitId = 1, Code = "in_review", IsFinal = false });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new StatusTransition { Id = 1, OrgUnitId = 1, FromStatusId = 10, ToStatusId = 20 } });
        _historyRepoMock.Setup(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CandidacyStatusHistory h, CancellationToken _) => h);

        var result = await _sut.TransitionStatusAsync(command);

        result.CurrentStatusId.Should().Be(20);
        result.IsActive.Should().BeTrue();
        _historyRepoMock.Verify(r => r.AddAsync(It.Is<CandidacyStatusHistory>(h =>
            h.FromStatusId == 10 && h.ToStatusId == 20 && h.Reason == "עבר בדיקה" && h.ChangedByUserId == 100),
            It.IsAny<CancellationToken>()), Times.Once);
        _candidacyRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Candidacy>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransitionStatusAsync_ToFinalStatus_SetsIsActiveFalse()
    {
        var candidacy = CreateCandidacyEntity(1, orgUnitId: 1);
        var command = new TransitionStatusCommand(1, 30, "התקבל", 100);

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(candidacy);
        _statusRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 10, OrgUnitId = 1, Code = "submitted", IsFinal = false });
        _statusRepoMock.Setup(r => r.GetByIdAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 30, OrgUnitId = 1, Code = "accepted", IsFinal = true });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new StatusTransition { Id = 1, OrgUnitId = 1, FromStatusId = 10, ToStatusId = 30 } });
        _historyRepoMock.Setup(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CandidacyStatusHistory h, CancellationToken _) => h);

        var result = await _sut.TransitionStatusAsync(command);

        result.CurrentStatusId.Should().Be(30);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task TransitionStatusAsync_DisallowedTransition_ThrowsBusinessRuleViolation()
    {
        var candidacy = CreateCandidacyEntity(1, orgUnitId: 1);
        var command = new TransitionStatusCommand(1, 99, null, 100);

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(candidacy);
        _statusRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 10, OrgUnitId = 1, Code = "submitted", IsFinal = false });
        _statusRepoMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 99, OrgUnitId = 1, Code = "other", IsFinal = false });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<StatusTransition>());

        var act = () => _sut.TransitionStatusAsync(command);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("*מעבר סטטוס*אינו מותר*");
    }

    [Fact]
    public async Task TransitionStatusAsync_CandidacyInFinalStatus_ThrowsBusinessRuleViolation()
    {
        var candidacy = CreateCandidacyEntity(1, orgUnitId: 1);
        candidacy.CurrentStatusId = 30;
        var command = new TransitionStatusCommand(1, 20, null, 100);

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(candidacy);
        _statusRepoMock.Setup(r => r.GetByIdAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 30, OrgUnitId = 1, Code = "accepted", IsFinal = true });

        var act = () => _sut.TransitionStatusAsync(command);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("*סטטוס סופי*");
    }

    [Fact]
    public async Task TransitionStatusAsync_InactiveCandidacy_ThrowsBusinessRuleViolation()
    {
        var candidacy = CreateCandidacyEntity(1, orgUnitId: 1);
        candidacy.IsActive = false;
        candidacy.CurrentStatusId = 10;
        var command = new TransitionStatusCommand(1, 20, null, 100);

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(candidacy);
        _statusRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 10, OrgUnitId = 1, Code = "submitted", IsFinal = false });

        var act = () => _sut.TransitionStatusAsync(command);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("*אינה פעילה*");
    }

    [Fact]
    public async Task TransitionStatusAsync_CandidacyNotFound_ThrowsNotFoundException()
    {
        var command = new TransitionStatusCommand(999, 20, null, 100);

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var act = () => _sut.TransitionStatusAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region StatusHistory

    [Fact]
    public async Task GetStatusHistoryAsync_ReturnsOrderedHistory()
    {
        var candidacy = CreateCandidacyEntity(1);
        var history = new List<CandidacyStatusHistory>
        {
            new() { Id = 1, CandidacyId = 1, FromStatusId = null, ToStatusId = 10, ChangedByUserId = 100, ChangedAt = DateTime.UtcNow.AddHours(-2) },
            new() { Id = 2, CandidacyId = 1, FromStatusId = 10, ToStatusId = 20, Reason = "עבר בדיקה", ChangedByUserId = 100, ChangedAt = DateTime.UtcNow.AddHours(-1) }
        };

        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(candidacy);
        _historyRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CandidacyStatusHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = (await _sut.GetStatusHistoryAsync(1)).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(2); // Most recent first
        result[1].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetStatusHistoryAsync_CandidacyNotFound_ThrowsNotFoundException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var act = () => _sut.GetStatusHistoryAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Helpers

    private void SetupValidCreateDependencies()
    {
        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1, IdNumber = "123456789", FirstName = "ישראל", LastName = "ישראלי" });
        _orgUnitRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1, Name = "יחידה" });
        _callRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 1, OrgUnitId = 1, Title = "קול קורא" });
        _candidacyRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _statusRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new StatusDefinition { Id = 10, OrgUnitId = 1, Code = "submitted", IsInitial = true } });
        _workflowRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new WorkflowDefinition { Id = 1, OrgUnitId = 1, Version = 1, IsActive = true, Name = "תהליך" } });
        _candidacyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Candidacy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy e, CancellationToken _) => e);
    }

    private static Candidacy CreateCandidacyEntity(int id, int contactId = 1, int orgUnitId = 1, int callId = 1) =>
        new()
        {
            Id = id,
            ContactId = contactId,
            OrgUnitId = orgUnitId,
            CallForCandidatesId = callId,
            CurrentStatusId = 10,
            IsActive = true,
            SubmittedAt = DateTime.UtcNow
        };

    #endregion
}
