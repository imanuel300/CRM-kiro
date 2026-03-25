using System.Linq.Expressions;
using CandidacyManagement.Application.Workflow;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Workflow;

public class WorkflowConfigServiceTests
{
    private readonly Mock<IRepository<WorkflowDefinition>> _workflowRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly Mock<IRepository<SubStatusDefinition>> _subStatusRepoMock;
    private readonly Mock<IRepository<StatusTransition>> _transitionRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly WorkflowConfigService _sut;

    public WorkflowConfigServiceTests()
    {
        _workflowRepoMock = new Mock<IRepository<WorkflowDefinition>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        _subStatusRepoMock = new Mock<IRepository<SubStatusDefinition>>();
        _transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();

        _sut = new WorkflowConfigService(
            _workflowRepoMock.Object,
            _statusRepoMock.Object,
            _subStatusRepoMock.Object,
            _transitionRepoMock.Object,
            _orgUnitRepoMock.Object);
    }

    #region ConfigureWorkflowAsync

    [Fact]
    public async Task ConfigureWorkflowAsync_CreatesNewVersion()
    {
        // Arrange
        SetupActiveOrgUnit(10);
        var existingWorkflow = new WorkflowDefinition { Id = 1, OrgUnitId = 10, Version = 1, IsActive = true };

        _workflowRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<WorkflowDefinition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existingWorkflow });

        _workflowRepoMock.Setup(r => r.AddAsync(It.IsAny<WorkflowDefinition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkflowDefinition w, CancellationToken _) => w);

        var command = new ConfigureWorkflowCommand(10, "תהליך מיון", true, true, false, true, null);

        // Act
        var result = await _sut.ConfigureWorkflowAsync(command);

        // Assert
        result.Version.Should().Be(2);
        result.ExamStepEnabled.Should().BeTrue();
        result.InterviewStepEnabled.Should().BeTrue();
        result.CommitteeStepEnabled.Should().BeFalse();
        result.IsActive.Should().BeTrue();

        existingWorkflow.IsActive.Should().BeFalse();
        _workflowRepoMock.Verify(r => r.UpdateAsync(existingWorkflow, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfigureWorkflowAsync_FirstWorkflow_Version1()
    {
        // Arrange
        SetupActiveOrgUnit(10);
        _workflowRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<WorkflowDefinition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<WorkflowDefinition>());

        _workflowRepoMock.Setup(r => r.AddAsync(It.IsAny<WorkflowDefinition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkflowDefinition w, CancellationToken _) => w);

        var command = new ConfigureWorkflowCommand(10, "תהליך ראשון", false, false, true, false, null);

        // Act
        var result = await _sut.ConfigureWorkflowAsync(command);

        // Assert
        result.Version.Should().Be(1);
        result.Name.Should().Be("תהליך ראשון");
    }

    [Fact]
    public async Task ConfigureWorkflowAsync_EmptyName_ThrowsValidationException()
    {
        // Arrange
        SetupActiveOrgUnit(10);
        var command = new ConfigureWorkflowCommand(10, "", false, false, false, false, null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ConfigureWorkflowAsync(command));
    }

    [Fact]
    public async Task ConfigureWorkflowAsync_InvalidOrgUnit_ThrowsNotFoundException()
    {
        // Arrange
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var command = new ConfigureWorkflowCommand(999, "test", false, false, false, false, null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ConfigureWorkflowAsync(command));
    }

    #endregion

    #region ConfigureStatusesAsync

    [Fact]
    public async Task ConfigureStatusesAsync_CreatesStatuses()
    {
        // Arrange
        SetupActiveOrgUnit(10);
        _statusRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<StatusDefinition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<StatusDefinition>());

        int idCounter = 1;
        _statusRepoMock.Setup(r => r.AddAsync(It.IsAny<StatusDefinition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StatusDefinition s, CancellationToken _) => { s.Id = idCounter++; return s; });

        _subStatusRepoMock.Setup(r => r.AddAsync(It.IsAny<SubStatusDefinition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubStatusDefinition s, CancellationToken _) => { s.Id = 100; return s; });

        var command = new ConfigureStatusesCommand(10, new[]
        {
            new ConfigureStatusDefinition("submitted", "הוגשה", CandidacyStatusCategory.Submitted, false, true, 1,
                new[] { new ConfigureSubStatusDefinition("pending", "ממתין לבדיקה") }),
            new ConfigureStatusDefinition("accepted", "התקבל", CandidacyStatusCategory.Accepted, true, false, 2, null)
        });

        // Act
        var result = (await _sut.ConfigureStatusesAsync(command)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Code.Should().Be("submitted");
        result[0].IsInitial.Should().BeTrue();
        result[0].SubStatuses.Should().HaveCount(1);
        result[1].Code.Should().Be("accepted");
        result[1].IsFinal.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigureStatusesAsync_NoInitialStatus_ThrowsValidationException()
    {
        // Arrange
        SetupActiveOrgUnit(10);
        var command = new ConfigureStatusesCommand(10, new[]
        {
            new ConfigureStatusDefinition("accepted", "התקבל", CandidacyStatusCategory.Accepted, true, false, 1, null)
        });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ConfigureStatusesAsync(command));
    }

    [Fact]
    public async Task ConfigureStatusesAsync_DuplicateCodes_ThrowsValidationException()
    {
        // Arrange
        SetupActiveOrgUnit(10);
        var command = new ConfigureStatusesCommand(10, new[]
        {
            new ConfigureStatusDefinition("same", "ראשון", CandidacyStatusCategory.Submitted, false, true, 1, null),
            new ConfigureStatusDefinition("same", "שני", CandidacyStatusCategory.InReview, false, false, 2, null)
        });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ConfigureStatusesAsync(command));
    }

    [Fact]
    public async Task ConfigureStatusesAsync_EmptyStatuses_ThrowsValidationException()
    {
        // Arrange
        SetupActiveOrgUnit(10);
        var command = new ConfigureStatusesCommand(10, Array.Empty<ConfigureStatusDefinition>());

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ConfigureStatusesAsync(command));
    }

    #endregion

    #region ConfigureTransitionsAsync

    [Fact]
    public async Task ConfigureTransitionsAsync_CreatesTransitions()
    {
        // Arrange
        SetupActiveOrgUnit(10);
        _transitionRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<StatusTransition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<StatusTransition>());

        var statuses = new[]
        {
            new StatusDefinition { Id = 1, OrgUnitId = 10, Code = "submitted" },
            new StatusDefinition { Id = 2, OrgUnitId = 10, Code = "in_review" }
        };
        _statusRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<StatusDefinition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(statuses);

        _transitionRepoMock.Setup(r => r.AddAsync(It.IsAny<StatusTransition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StatusTransition t, CancellationToken _) => { t.Id = 1; return t; });

        var command = new ConfigureTransitionsCommand(10, new[]
        {
            new ConfigureTransitionDefinition("submitted", "in_review", null, false, null)
        });

        // Act
        var result = (await _sut.ConfigureTransitionsAsync(command)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].FromStatusCode.Should().Be("submitted");
        result[0].ToStatusCode.Should().Be("in_review");
    }

    [Fact]
    public async Task ConfigureTransitionsAsync_InvalidFromStatus_ThrowsValidationException()
    {
        // Arrange
        SetupActiveOrgUnit(10);
        _transitionRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<StatusTransition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<StatusTransition>());

        _statusRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<StatusDefinition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new StatusDefinition { Id = 1, OrgUnitId = 10, Code = "submitted" } });

        var command = new ConfigureTransitionsCommand(10, new[]
        {
            new ConfigureTransitionDefinition("nonexistent", "submitted", null, false, null)
        });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ConfigureTransitionsAsync(command));
    }

    #endregion

    #region Helpers

    private void SetupActiveOrgUnit(int id)
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = id, Name = "Test", IsActive = true });
    }

    #endregion
}
