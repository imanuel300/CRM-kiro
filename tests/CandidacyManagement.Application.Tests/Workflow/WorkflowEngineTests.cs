using System.Linq.Expressions;
using CandidacyManagement.Application.Workflow;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Events;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using MediatR;
using Moq;

namespace CandidacyManagement.Application.Tests.Workflow;

public class WorkflowEngineTests
{
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly Mock<IRepository<StatusTransition>> _transitionRepoMock;
    private readonly Mock<IRepository<CandidacyStatusHistory>> _historyRepoMock;
    private readonly Mock<IRepository<WorkflowDefinition>> _workflowRepoMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly WorkflowEngine _sut;

    public WorkflowEngineTests()
    {
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        _transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        _historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();
        _workflowRepoMock = new Mock<IRepository<WorkflowDefinition>>();
        _mediatorMock = new Mock<IMediator>();

        _sut = new WorkflowEngine(
            _candidacyRepoMock.Object,
            _statusRepoMock.Object,
            _transitionRepoMock.Object,
            _historyRepoMock.Object,
            _workflowRepoMock.Object,
            _mediatorMock.Object);
    }

    #region CanTransitionAsync

    [Fact]
    public async Task CanTransitionAsync_WithValidTransition_ReturnsTrue()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = true };
        var currentStatus = new StatusDefinition { Id = 100, OrgUnitId = 10, Code = "submitted", IsFinal = false };
        var targetStatus = new StatusDefinition { Id = 200, OrgUnitId = 10, Code = "in_review" };
        var transition = new StatusTransition { OrgUnitId = 10, FromStatusId = 100, ToStatusId = 200 };

        SetupCandidacy(candidacy);
        SetupStatus(currentStatus);
        SetupStatusByCode(10, "in_review", targetStatus);
        SetupTransitions(10, 100, new[] { transition });

        // Act
        var result = await _sut.CanTransitionAsync(1, "in_review");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanTransitionAsync_WithInvalidTransition_ReturnsFalse()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = true };
        var currentStatus = new StatusDefinition { Id = 100, OrgUnitId = 10, Code = "submitted", IsFinal = false };
        var targetStatus = new StatusDefinition { Id = 300, OrgUnitId = 10, Code = "accepted" };
        var transition = new StatusTransition { OrgUnitId = 10, FromStatusId = 100, ToStatusId = 200 };

        SetupCandidacy(candidacy);
        SetupStatus(currentStatus);
        SetupStatusByCode(10, "accepted", targetStatus);
        SetupTransitions(10, 100, new[] { transition });

        // Act
        var result = await _sut.CanTransitionAsync(1, "accepted");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanTransitionAsync_WhenCandidacyInactive_ReturnsFalse()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = false };
        SetupCandidacy(candidacy);

        // Act
        var result = await _sut.CanTransitionAsync(1, "in_review");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanTransitionAsync_WhenCurrentStatusIsFinal_ReturnsFalse()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = true };
        var currentStatus = new StatusDefinition { Id = 100, OrgUnitId = 10, Code = "accepted", IsFinal = true };

        SetupCandidacy(candidacy);
        SetupStatus(currentStatus);

        // Act
        var result = await _sut.CanTransitionAsync(1, "in_review");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanTransitionAsync_WhenCandidacyNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CanTransitionAsync(999, "in_review"));
    }

    #endregion

    #region ExecuteTransitionAsync

    [Fact]
    public async Task ExecuteTransitionAsync_WithValidTransition_ReturnsSuccess()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = true };
        var currentStatus = new StatusDefinition { Id = 100, OrgUnitId = 10, Code = "submitted", IsFinal = false };
        var targetStatus = new StatusDefinition { Id = 200, OrgUnitId = 10, Code = "in_review", IsFinal = false };
        var transition = new StatusTransition { OrgUnitId = 10, FromStatusId = 100, ToStatusId = 200 };

        SetupCandidacy(candidacy);
        SetupStatus(currentStatus);
        SetupStatusByCode(10, "in_review", targetStatus);
        SetupTransitions(10, 100, new[] { transition });
        _historyRepoMock.Setup(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CandidacyStatusHistory h, CancellationToken _) => h);

        // Act
        var result = await _sut.ExecuteTransitionAsync(1, "in_review", "בדיקה", 42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be("in_review");
        candidacy.CurrentStatusId.Should().Be(200);
        candidacy.IsActive.Should().BeTrue();

        _historyRepoMock.Verify(r => r.AddAsync(
            It.Is<CandidacyStatusHistory>(h =>
                h.CandidacyId == 1 &&
                h.FromStatusId == 100 &&
                h.ToStatusId == 200 &&
                h.ChangedByUserId == 42),
            It.IsAny<CancellationToken>()), Times.Once);

        _mediatorMock.Verify(m => m.Publish(
            It.Is<CandidacyStatusChangedEvent>(e =>
                e.CandidacyId == 1 && e.ToStatusCode == "in_review"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteTransitionAsync_ToFinalStatus_MarksCandidacyInactive()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = true };
        var currentStatus = new StatusDefinition { Id = 100, OrgUnitId = 10, Code = "committee", IsFinal = false };
        var targetStatus = new StatusDefinition { Id = 200, OrgUnitId = 10, Code = "accepted", IsFinal = true };
        var transition = new StatusTransition { OrgUnitId = 10, FromStatusId = 100, ToStatusId = 200 };

        SetupCandidacy(candidacy);
        SetupStatus(currentStatus);
        SetupStatusByCode(10, "accepted", targetStatus);
        SetupTransitions(10, 100, new[] { transition });
        _historyRepoMock.Setup(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CandidacyStatusHistory h, CancellationToken _) => h);

        // Act
        var result = await _sut.ExecuteTransitionAsync(1, "accepted", null, 42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        candidacy.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteTransitionAsync_WhenInactive_ReturnsNotAllowed()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = false };
        SetupCandidacy(candidacy);

        // Act
        var result = await _sut.ExecuteTransitionAsync(1, "in_review", null, 42);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteTransitionAsync_WhenFinalStatus_ReturnsNotAllowed()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = true };
        var currentStatus = new StatusDefinition { Id = 100, OrgUnitId = 10, Code = "accepted", IsFinal = true };

        SetupCandidacy(candidacy);
        SetupStatus(currentStatus);

        // Act
        var result = await _sut.ExecuteTransitionAsync(1, "in_review", null, 42);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteTransitionAsync_WhenReasonRequired_AndMissing_ReturnsFailed()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = true };
        var currentStatus = new StatusDefinition { Id = 100, OrgUnitId = 10, Code = "submitted", IsFinal = false };
        var targetStatus = new StatusDefinition { Id = 200, OrgUnitId = 10, Code = "rejected", IsFinal = true };
        var transition = new StatusTransition { OrgUnitId = 10, FromStatusId = 100, ToStatusId = 200, RequiresReason = true };

        SetupCandidacy(candidacy);
        SetupStatus(currentStatus);
        SetupStatusByCode(10, "rejected", targetStatus);
        SetupTransitions(10, 100, new[] { transition });

        // Act
        var result = await _sut.ExecuteTransitionAsync(1, "rejected", null, 42);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("סיבה");
    }

    #endregion

    #region GetAllowedTransitionsAsync

    [Fact]
    public async Task GetAllowedTransitionsAsync_ReturnsTargetStatusCodes()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = true };
        var currentStatus = new StatusDefinition { Id = 100, OrgUnitId = 10, Code = "submitted", IsFinal = false };
        var transitions = new[]
        {
            new StatusTransition { OrgUnitId = 10, FromStatusId = 100, ToStatusId = 200 },
            new StatusTransition { OrgUnitId = 10, FromStatusId = 100, ToStatusId = 300 }
        };
        var targetStatuses = new[]
        {
            new StatusDefinition { Id = 200, OrgUnitId = 10, Code = "in_review" },
            new StatusDefinition { Id = 300, OrgUnitId = 10, Code = "rejected" }
        };

        SetupCandidacy(candidacy);
        SetupStatus(currentStatus);
        SetupTransitions(10, 100, transitions);

        _statusRepoMock.Setup(r => r.FindAsync(
                It.Is<Expression<Func<StatusDefinition, bool>>>(e => true),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetStatuses);

        // Act
        var result = (await _sut.GetAllowedTransitionsAsync(1)).ToList();

        // Assert
        result.Should().Contain("in_review");
        result.Should().Contain("rejected");
    }

    [Fact]
    public async Task GetAllowedTransitionsAsync_WhenInactive_ReturnsEmpty()
    {
        // Arrange
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CurrentStatusId = 100, IsActive = false };
        SetupCandidacy(candidacy);

        // Act
        var result = await _sut.GetAllowedTransitionsAsync(1);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetWorkflowDefinitionAsync

    [Fact]
    public async Task GetWorkflowDefinitionAsync_ReturnsLatestActiveVersion()
    {
        // Arrange
        var workflows = new[]
        {
            new WorkflowDefinition { Id = 1, OrgUnitId = 10, Name = "v1", Version = 1, IsActive = false },
            new WorkflowDefinition { Id = 2, OrgUnitId = 10, Name = "v2", Version = 2, IsActive = true, ExamStepEnabled = true }
        };

        _workflowRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<WorkflowDefinition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflows.Where(w => w.IsActive));

        // Act
        var result = await _sut.GetWorkflowDefinitionAsync(10);

        // Assert
        result.Should().NotBeNull();
        result!.Version.Should().Be(2);
        result.ExamStepEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetWorkflowDefinitionAsync_WhenNone_ReturnsNull()
    {
        // Arrange
        _workflowRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<WorkflowDefinition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<WorkflowDefinition>());

        // Act
        var result = await _sut.GetWorkflowDefinitionAsync(10);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Helpers

    private void SetupCandidacy(Candidacy candidacy)
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(candidacy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
    }

    private void SetupStatus(StatusDefinition status)
    {
        _statusRepoMock.Setup(r => r.GetByIdAsync(status.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);
    }

    private void SetupStatusByCode(int orgUnitId, string code, StatusDefinition status)
    {
        _statusRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<StatusDefinition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { status });
    }

    private void SetupTransitions(int orgUnitId, int fromStatusId, IEnumerable<StatusTransition> transitions)
    {
        _transitionRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<StatusTransition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transitions);
    }

    #endregion
}
