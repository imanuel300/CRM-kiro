using CandidacyManagement.Application.Candidacies;
using CandidacyManagement.Application.Notifications;
using CandidacyManagement.Application.Screening;
using CandidacyManagement.Application.ThresholdChecks;
using CandidacyManagement.Application.Workflow;
using CandidacyManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Integration;

/// <summary>
/// Integration tests for the full screening orchestration flow.
/// Tests end-to-end scenarios: initiate → threshold → exam → interview → committee → accepted
/// Validates: Requirements 1.2, 3.3, 9.3
/// </summary>
public class EndToEndScreeningTests
{
    private readonly Mock<IWorkflowEngine> _workflowEngine;
    private readonly Mock<IWorkflowConfigService> _workflowConfig;
    private readonly Mock<ICandidacyService> _candidacyService;
    private readonly Mock<IThresholdCheckService> _thresholdCheck;
    private readonly Mock<INotificationService> _notification;
    private readonly ScreeningOrchestrator _sut;

    private const int CandidacyId = 1;
    private const int OrgUnitId = 10;
    private const int UserId = 42;

    public EndToEndScreeningTests()
    {
        _workflowEngine = new Mock<IWorkflowEngine>();
        _workflowConfig = new Mock<IWorkflowConfigService>();
        _candidacyService = new Mock<ICandidacyService>();
        _thresholdCheck = new Mock<IThresholdCheckService>();
        _notification = new Mock<INotificationService>();

        _sut = new ScreeningOrchestrator(
            _workflowEngine.Object,
            _workflowConfig.Object,
            _candidacyService.Object,
            _thresholdCheck.Object,
            _notification.Object);
    }

    #region 1. Full Screening Flow: Initiate → Threshold → Exam → Interview → Committee → Accepted

    [Fact]
    public async Task FullFlow_InitiateWithThreshold_PassesThresholdAndMovesToExam()
    {
        // Arrange - full workflow with all stages
        var candidacy = CreateCandidacy();
        var workflow = CreateFullWorkflow();
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupThresholdPassed();
        SetupTransitionAllowed("exam_pending");

        // Act - initiate screening
        var result = await _sut.InitiateCandidacyScreeningAsync(CandidacyId, UserId);

        // Assert - threshold passed, moved to exam
        result.IsSuccess.Should().BeTrue();
        result.CurrentStage.Should().Be("threshold_check");
        result.NextStage.Should().Be("exam");
        _thresholdCheck.Verify(t => t.CheckAllAsync(CandidacyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FullFlow_ExamCompleted_MovesToInterview()
    {
        // Arrange
        var candidacy = CreateCandidacy();
        var workflow = CreateFullWorkflow();
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupTransitionAllowed("interview_pending");

        // Act - complete exam stage
        var result = await _sut.ProcessStageCompletionAsync(CandidacyId, "exam", UserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.CurrentStage.Should().Be("exam");
        result.NextStage.Should().Be("interview");
    }

    [Fact]
    public async Task FullFlow_InterviewCompleted_MovesToCommittee()
    {
        // Arrange
        var candidacy = CreateCandidacy();
        var workflow = CreateFullWorkflow();
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupTransitionAllowed("committee_pending");

        // Act - complete interview stage
        var result = await _sut.ProcessStageCompletionAsync(CandidacyId, "interview", UserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.CurrentStage.Should().Be("interview");
        result.NextStage.Should().Be("committee");
    }

    [Fact]
    public async Task FullFlow_CommitteeCompleted_ScreeningFinished()
    {
        // Arrange
        var candidacy = CreateCandidacy();
        var workflow = CreateFullWorkflow();
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);

        // Act - complete committee (last stage)
        var result = await _sut.ProcessStageCompletionAsync(CandidacyId, "committee", UserId);

        // Assert - no next stage, screening complete
        result.IsSuccess.Should().BeTrue();
        result.CurrentStage.Should().Be("committee");
        result.NextStage.Should().BeNull();
    }

    [Fact]
    public async Task FullFlow_EachStageTransition_TriggersNotification()
    {
        // Arrange
        var candidacy = CreateCandidacy();
        var workflow = CreateFullWorkflow();
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupThresholdPassed();
        SetupTransitionAllowed("exam_pending");
        SetupTransitionAllowed("interview_pending");
        SetupTransitionAllowed("committee_pending");

        // Act - run through all stages
        await _sut.InitiateCandidacyScreeningAsync(CandidacyId, UserId);
        await _sut.ProcessStageCompletionAsync(CandidacyId, "exam", UserId);
        await _sut.ProcessStageCompletionAsync(CandidacyId, "interview", UserId);

        // Assert - notifications sent for threshold→exam, exam→interview, interview→committee
        _notification.Verify(n => n.TriggerAsync(
            OrgUnitId, CandidacyId, TriggerEventType.StatusChange,
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    #endregion

    #region 2. Threshold Failure Flow

    [Fact]
    public async Task ThresholdFailure_InitiateScreening_RejectsAndNotifies()
    {
        // Arrange
        var candidacy = CreateCandidacy();
        var workflow = CreateFullWorkflow();
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupThresholdFailed();

        // Act
        var result = await _sut.InitiateCandidacyScreeningAsync(CandidacyId, UserId);

        // Assert - candidacy rejected at threshold
        result.IsSuccess.Should().BeFalse();
        result.CurrentStage.Should().Be("threshold_check");
        result.NextStage.Should().BeNull();
        result.ErrorMessage.Should().Contain("תנאי הסף");

        // Notification sent for rejection
        _notification.Verify(n => n.TriggerAsync(
            OrgUnitId, CandidacyId, TriggerEventType.StatusChange,
            It.Is<Dictionary<string, string>>(d =>
                d["from_stage"] == "threshold_check" && d["to_stage"] == "completed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region 3. Skipped Stages Flow

    [Fact]
    public async Task SkippedStages_ExamAndCommitteeOnly_SkipsInterview()
    {
        // Arrange - workflow with exam + committee, no interview
        var candidacy = CreateCandidacy();
        var workflow = CreateWorkflow(
            thresholdEnabled: false, examEnabled: true,
            interviewEnabled: false, committeeEnabled: true);
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupTransitionAllowed("exam_pending");
        SetupTransitionAllowed("committee_pending");

        // Act - initiate goes to exam
        var initResult = await _sut.InitiateCandidacyScreeningAsync(CandidacyId, UserId);
        initResult.IsSuccess.Should().BeTrue();
        initResult.NextStage.Should().Be("exam");

        // Act - exam completed goes directly to committee (skips interview)
        var examResult = await _sut.ProcessStageCompletionAsync(CandidacyId, "exam", UserId);

        // Assert
        examResult.IsSuccess.Should().BeTrue();
        examResult.CurrentStage.Should().Be("exam");
        examResult.NextStage.Should().Be("committee");
    }

    [Fact]
    public async Task SkippedStages_OnlyCommittee_GoesDirectlyToCommittee()
    {
        // Arrange - workflow with only committee
        var candidacy = CreateCandidacy();
        var workflow = CreateWorkflow(
            thresholdEnabled: false, examEnabled: false,
            interviewEnabled: false, committeeEnabled: true);
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupTransitionAllowed("committee_pending");

        // Act
        var result = await _sut.InitiateCandidacyScreeningAsync(CandidacyId, UserId);

        // Assert - goes directly to committee
        result.IsSuccess.Should().BeTrue();
        result.CurrentStage.Should().Be("initiated");
        result.NextStage.Should().Be("committee");
    }

    #endregion

    #region 4. Notification Integration

    [Fact]
    public async Task Notification_ThresholdToExam_ContainsCorrectStageVariables()
    {
        // Arrange
        var candidacy = CreateCandidacy();
        var workflow = CreateFullWorkflow();
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupThresholdPassed();
        SetupTransitionAllowed("exam_pending");

        // Act
        await _sut.InitiateCandidacyScreeningAsync(CandidacyId, UserId);

        // Assert - notification variables contain from/to stage info
        _notification.Verify(n => n.TriggerAsync(
            OrgUnitId, CandidacyId, TriggerEventType.StatusChange,
            It.Is<Dictionary<string, string>>(d =>
                d["from_stage"] == "threshold_check" && d["to_stage"] == "exam"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Notification_ExamToInterview_ContainsCorrectStageVariables()
    {
        // Arrange
        var candidacy = CreateCandidacy();
        var workflow = CreateFullWorkflow();
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupTransitionAllowed("interview_pending");

        // Act
        await _sut.ProcessStageCompletionAsync(CandidacyId, "exam", UserId);

        // Assert
        _notification.Verify(n => n.TriggerAsync(
            OrgUnitId, CandidacyId, TriggerEventType.StatusChange,
            It.Is<Dictionary<string, string>>(d =>
                d["from_stage"] == "exam" && d["to_stage"] == "interview"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region 5. Custom Step Order

    [Fact]
    public async Task CustomStepOrder_InterviewBeforeExam_ExecutesInCustomOrder()
    {
        // Arrange - custom order: interview → exam → committee
        var candidacy = CreateCandidacy();
        var workflow = CreateWorkflow(
            thresholdEnabled: false, examEnabled: true,
            interviewEnabled: true, committeeEnabled: true,
            stepOrder: "interview,exam,committee");
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupTransitionAllowed("interview_pending");
        SetupTransitionAllowed("exam_pending");
        SetupTransitionAllowed("committee_pending");

        // Act - initiate goes to interview first (custom order)
        var initResult = await _sut.InitiateCandidacyScreeningAsync(CandidacyId, UserId);
        initResult.IsSuccess.Should().BeTrue();
        initResult.NextStage.Should().Be("interview");

        // Act - interview completed goes to exam (custom order)
        var interviewResult = await _sut.ProcessStageCompletionAsync(CandidacyId, "interview", UserId);
        interviewResult.IsSuccess.Should().BeTrue();
        interviewResult.NextStage.Should().Be("exam");

        // Act - exam completed goes to committee
        var examResult = await _sut.ProcessStageCompletionAsync(CandidacyId, "exam", UserId);
        examResult.IsSuccess.Should().BeTrue();
        examResult.NextStage.Should().Be("committee");
    }

    [Fact]
    public async Task CustomStepOrder_ThresholdFirstThenCustomOrder_RespectsThresholdAndOrder()
    {
        // Arrange - threshold enabled + custom order for remaining stages
        var candidacy = CreateCandidacy();
        var workflow = CreateWorkflow(
            thresholdEnabled: true, examEnabled: true,
            interviewEnabled: true, committeeEnabled: true,
            stepOrder: "threshold_check,interview,exam,committee");
        SetupCandidacy(candidacy);
        SetupWorkflow(workflow);
        SetupThresholdPassed();
        SetupTransitionAllowed("interview_pending");

        // Act - initiate: threshold passes, moves to interview (not exam)
        var result = await _sut.InitiateCandidacyScreeningAsync(CandidacyId, UserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.CurrentStage.Should().Be("threshold_check");
        result.NextStage.Should().Be("interview");
    }

    #endregion

    #region Helpers

    private static CandidacyDto CreateCandidacy() =>
        new(CandidacyId, 100, OrgUnitId, 200, 1, null, null, true,
            DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);

    private static WorkflowDefinitionDto CreateFullWorkflow() =>
        CreateWorkflow(thresholdEnabled: true, examEnabled: true,
            interviewEnabled: true, committeeEnabled: true);

    private static WorkflowDefinitionDto CreateWorkflow(
        bool thresholdEnabled = false,
        bool examEnabled = false,
        bool interviewEnabled = false,
        bool committeeEnabled = false,
        string? stepOrder = null) =>
        new(1, OrgUnitId, "Test Workflow",
            examEnabled, interviewEnabled, committeeEnabled, thresholdEnabled,
            stepOrder, 1, true, DateTime.UtcNow);

    private void SetupCandidacy(CandidacyDto candidacy) =>
        _candidacyService.Setup(s => s.GetByIdAsync(CandidacyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

    private void SetupWorkflow(WorkflowDefinitionDto workflow) =>
        _workflowEngine.Setup(w => w.GetWorkflowDefinitionAsync(OrgUnitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);

    private void SetupThresholdPassed() =>
        _thresholdCheck.Setup(t => t.CheckAllAsync(CandidacyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CheckAllResultDto(CandidacyId, true, Enumerable.Empty<ThresholdCheckResultDto>()));

    private void SetupThresholdFailed() =>
        _thresholdCheck.Setup(t => t.CheckAllAsync(CandidacyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CheckAllResultDto(CandidacyId, false, Enumerable.Empty<ThresholdCheckResultDto>()));

    private void SetupTransitionAllowed(string targetStatusCode)
    {
        _workflowEngine.Setup(w => w.CanTransitionAsync(CandidacyId, targetStatusCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workflowEngine.Setup(w => w.ExecuteTransitionAsync(CandidacyId, targetStatusCode, It.IsAny<string?>(), UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StatusTransitionResult.Success(targetStatusCode));
    }

    #endregion
}
