using CandidacyManagement.Application.Candidacies;
using CandidacyManagement.Application.Notifications;
using CandidacyManagement.Application.Screening;
using CandidacyManagement.Application.ThresholdChecks;
using CandidacyManagement.Application.Workflow;
using CandidacyManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Screening;

public class ScreeningOrchestratorTests
{
    private readonly Mock<IWorkflowEngine> _workflowEngineMock;
    private readonly Mock<IWorkflowConfigService> _workflowConfigMock;
    private readonly Mock<ICandidacyService> _candidacyServiceMock;
    private readonly Mock<IThresholdCheckService> _thresholdCheckMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly ScreeningOrchestrator _sut;

    public ScreeningOrchestratorTests()
    {
        _workflowEngineMock = new Mock<IWorkflowEngine>();
        _workflowConfigMock = new Mock<IWorkflowConfigService>();
        _candidacyServiceMock = new Mock<ICandidacyService>();
        _thresholdCheckMock = new Mock<IThresholdCheckService>();
        _notificationMock = new Mock<INotificationService>();

        _sut = new ScreeningOrchestrator(
            _workflowEngineMock.Object,
            _workflowConfigMock.Object,
            _candidacyServiceMock.Object,
            _thresholdCheckMock.Object,
            _notificationMock.Object);
    }

    #region ProcessStageCompletionAsync

    [Fact]
    public async Task ProcessStageCompletion_WithValidTransition_MovesToNextStage()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        var workflow = CreateWorkflow(10, examEnabled: true, interviewEnabled: true);

        SetupCandidacy(candidacy);
        SetupWorkflow(10, workflow);
        _workflowEngineMock.Setup(w => w.CanTransitionAsync(1, "interview_pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workflowEngineMock.Setup(w => w.ExecuteTransitionAsync(1, "interview_pending", It.IsAny<string?>(), 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StatusTransitionResult.Success("interview_pending"));

        // Act
        var result = await _sut.ProcessStageCompletionAsync(1, "exam", 42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.CurrentStage.Should().Be("exam");
        result.NextStage.Should().Be("interview");
    }

    [Fact]
    public async Task ProcessStageCompletion_LastStage_ReturnsSuccessWithNoNextStage()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        var workflow = CreateWorkflow(10, examEnabled: true, interviewEnabled: false, committeeEnabled: false);

        SetupCandidacy(candidacy);
        SetupWorkflow(10, workflow);

        // Act
        var result = await _sut.ProcessStageCompletionAsync(1, "exam", 42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.CurrentStage.Should().Be("exam");
        result.NextStage.Should().BeNull();
    }

    [Fact]
    public async Task ProcessStageCompletion_NoWorkflow_ReturnsNoWorkflowDefined()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        SetupCandidacy(candidacy);
        _workflowEngineMock.Setup(w => w.GetWorkflowDefinitionAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkflowDefinitionDto?)null);

        // Act
        var result = await _sut.ProcessStageCompletionAsync(1, "exam", 42);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("לא הוגדר תהליך מיון");
    }

    [Fact]
    public async Task ProcessStageCompletion_TransitionNotAllowed_ReturnsFailed()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        var workflow = CreateWorkflow(10, examEnabled: true, interviewEnabled: true);

        SetupCandidacy(candidacy);
        SetupWorkflow(10, workflow);
        _workflowEngineMock.Setup(w => w.CanTransitionAsync(1, "interview_pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ProcessStageCompletionAsync(1, "exam", 42);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("אינו מותר");
    }

    [Fact]
    public async Task ProcessStageCompletion_SendsNotificationOnSuccess()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        var workflow = CreateWorkflow(10, examEnabled: true, interviewEnabled: true);

        SetupCandidacy(candidacy);
        SetupWorkflow(10, workflow);
        _workflowEngineMock.Setup(w => w.CanTransitionAsync(1, "interview_pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workflowEngineMock.Setup(w => w.ExecuteTransitionAsync(1, "interview_pending", It.IsAny<string?>(), 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StatusTransitionResult.Success("interview_pending"));

        // Act
        await _sut.ProcessStageCompletionAsync(1, "exam", 42);

        // Assert
        _notificationMock.Verify(n => n.TriggerAsync(
            10, 1, TriggerEventType.StatusChange,
            It.Is<Dictionary<string, string>>(d => d["from_stage"] == "exam" && d["to_stage"] == "interview"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessStageCompletion_TransitionFails_ReturnsFailed()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        var workflow = CreateWorkflow(10, examEnabled: true, interviewEnabled: true);

        SetupCandidacy(candidacy);
        SetupWorkflow(10, workflow);
        _workflowEngineMock.Setup(w => w.CanTransitionAsync(1, "interview_pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workflowEngineMock.Setup(w => w.ExecuteTransitionAsync(1, "interview_pending", It.IsAny<string?>(), 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StatusTransitionResult.Failed("שגיאה פנימית"));

        // Act
        var result = await _sut.ProcessStageCompletionAsync(1, "exam", 42);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("שגיאה פנימית");
    }

    #endregion

    #region InitiateCandidacyScreeningAsync

    [Fact]
    public async Task InitiateScreening_WithThresholdCheck_RunsThresholdFirst()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        var workflow = CreateWorkflow(10, thresholdEnabled: true, examEnabled: true);
        var thresholdResult = new CheckAllResultDto(1, true, Enumerable.Empty<ThresholdCheckResultDto>());

        SetupCandidacy(candidacy);
        SetupWorkflow(10, workflow);
        _thresholdCheckMock.Setup(t => t.CheckAllAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(thresholdResult);
        _workflowEngineMock.Setup(w => w.CanTransitionAsync(1, "exam_pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workflowEngineMock.Setup(w => w.ExecuteTransitionAsync(1, "exam_pending", It.IsAny<string?>(), 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StatusTransitionResult.Success("exam_pending"));

        // Act
        var result = await _sut.InitiateCandidacyScreeningAsync(1, 42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.CurrentStage.Should().Be("threshold_check");
        result.NextStage.Should().Be("exam");
        _thresholdCheckMock.Verify(t => t.CheckAllAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitiateScreening_ThresholdFails_ReturnsThresholdFailed()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        var workflow = CreateWorkflow(10, thresholdEnabled: true, examEnabled: true);
        var thresholdResult = new CheckAllResultDto(1, false, Enumerable.Empty<ThresholdCheckResultDto>());

        SetupCandidacy(candidacy);
        SetupWorkflow(10, workflow);
        _thresholdCheckMock.Setup(t => t.CheckAllAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(thresholdResult);

        // Act
        var result = await _sut.InitiateCandidacyScreeningAsync(1, 42);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.CurrentStage.Should().Be("threshold_check");
        result.ErrorMessage.Should().Contain("תנאי הסף");
    }

    [Fact]
    public async Task InitiateScreening_NoThreshold_MovesToFirstStage()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        var workflow = CreateWorkflow(10, thresholdEnabled: false, examEnabled: true, interviewEnabled: true);

        SetupCandidacy(candidacy);
        SetupWorkflow(10, workflow);
        _workflowEngineMock.Setup(w => w.CanTransitionAsync(1, "exam_pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workflowEngineMock.Setup(w => w.ExecuteTransitionAsync(1, "exam_pending", It.IsAny<string?>(), 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StatusTransitionResult.Success("exam_pending"));

        // Act
        var result = await _sut.InitiateCandidacyScreeningAsync(1, 42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.CurrentStage.Should().Be("initiated");
        result.NextStage.Should().Be("exam");
    }

    [Fact]
    public async Task InitiateScreening_NoWorkflow_ReturnsNoWorkflowDefined()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        SetupCandidacy(candidacy);
        _workflowEngineMock.Setup(w => w.GetWorkflowDefinitionAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkflowDefinitionDto?)null);

        // Act
        var result = await _sut.InitiateCandidacyScreeningAsync(1, 42);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("לא הוגדר תהליך מיון");
    }

    [Fact]
    public async Task InitiateScreening_ThresholdPassed_SendsNotification()
    {
        // Arrange
        var candidacy = CreateCandidacy(1, 10);
        var workflow = CreateWorkflow(10, thresholdEnabled: true, examEnabled: true);
        var thresholdResult = new CheckAllResultDto(1, true, Enumerable.Empty<ThresholdCheckResultDto>());

        SetupCandidacy(candidacy);
        SetupWorkflow(10, workflow);
        _thresholdCheckMock.Setup(t => t.CheckAllAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(thresholdResult);
        _workflowEngineMock.Setup(w => w.CanTransitionAsync(1, "exam_pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workflowEngineMock.Setup(w => w.ExecuteTransitionAsync(1, "exam_pending", It.IsAny<string?>(), 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StatusTransitionResult.Success("exam_pending"));

        // Act
        await _sut.InitiateCandidacyScreeningAsync(1, 42);

        // Assert
        _notificationMock.Verify(n => n.TriggerAsync(
            10, 1, TriggerEventType.StatusChange,
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetActiveStages / GetNextStage (static helpers)

    [Fact]
    public void GetActiveStages_ReturnsOnlyEnabledStages()
    {
        // Arrange
        var workflow = CreateWorkflow(10, thresholdEnabled: true, examEnabled: true, interviewEnabled: false, committeeEnabled: true);

        // Act
        var stages = ScreeningOrchestrator.GetActiveStages(workflow);

        // Assert
        stages.Should().BeEquivalentTo(new[] { "threshold_check", "exam", "committee" });
        stages.Should().NotContain("interview");
    }

    [Fact]
    public void GetActiveStages_WithCustomStepOrder_RespectsOrder()
    {
        // Arrange
        var workflow = CreateWorkflow(10,
            thresholdEnabled: true, examEnabled: true, interviewEnabled: true, committeeEnabled: true,
            stepOrder: "interview,exam,threshold_check,committee");

        // Act
        var stages = ScreeningOrchestrator.GetActiveStages(workflow);

        // Assert
        stages.Should().Equal("interview", "exam", "threshold_check", "committee");
    }

    [Fact]
    public void GetNextStage_ReturnsCorrectNextStage()
    {
        var stages = new List<string> { "threshold_check", "exam", "interview", "committee" };

        ScreeningOrchestrator.GetNextStage(stages, "exam").Should().Be("interview");
        ScreeningOrchestrator.GetNextStage(stages, "threshold_check").Should().Be("exam");
        ScreeningOrchestrator.GetNextStage(stages, "committee").Should().BeNull();
        ScreeningOrchestrator.GetNextStage(stages, "unknown").Should().BeNull();
    }

    #endregion

    #region Helpers

    private static CandidacyDto CreateCandidacy(int id, int orgUnitId) =>
        new(id, 100, orgUnitId, 200, 1, null, null, true, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);

    private static WorkflowDefinitionDto CreateWorkflow(
        int orgUnitId,
        bool thresholdEnabled = false,
        bool examEnabled = false,
        bool interviewEnabled = false,
        bool committeeEnabled = false,
        string? stepOrder = null) =>
        new(1, orgUnitId, "Test Workflow",
            examEnabled, interviewEnabled, committeeEnabled, thresholdEnabled,
            stepOrder, 1, true, DateTime.UtcNow);

    private void SetupCandidacy(CandidacyDto candidacy)
    {
        _candidacyServiceMock.Setup(s => s.GetByIdAsync(candidacy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
    }

    private void SetupWorkflow(int orgUnitId, WorkflowDefinitionDto workflow)
    {
        _workflowEngineMock.Setup(w => w.GetWorkflowDefinitionAsync(orgUnitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);
    }

    #endregion
}
