using CandidacyManagement.Application.Candidacies;
using CandidacyManagement.Application.Notifications;
using CandidacyManagement.Application.ThresholdChecks;
using CandidacyManagement.Application.Workflow;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.Screening;

/// <summary>
/// מתזמר תהליך מיון - מחבר את מנוע התהליך לכל שלבי המיון
/// ומפעיל דיוורים אוטומטיים בכל מעבר סטטוס
/// </summary>
public class ScreeningOrchestrator : IScreeningOrchestrator
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowConfigService _workflowConfigService;
    private readonly ICandidacyService _candidacyService;
    private readonly IThresholdCheckService _thresholdCheckService;
    private readonly INotificationService _notificationService;

    // Screening stage constants matching workflow step order
    internal const string StageThresholdCheck = "threshold_check";
    internal const string StageExam = "exam";
    internal const string StageInterview = "interview";
    internal const string StageCommittee = "committee";

    public ScreeningOrchestrator(
        IWorkflowEngine workflowEngine,
        IWorkflowConfigService workflowConfigService,
        ICandidacyService candidacyService,
        IThresholdCheckService thresholdCheckService,
        INotificationService notificationService)
    {
        _workflowEngine = workflowEngine;
        _workflowConfigService = workflowConfigService;
        _candidacyService = candidacyService;
        _thresholdCheckService = thresholdCheckService;
        _notificationService = notificationService;
    }

    public async Task<ScreeningStageResult> ProcessStageCompletionAsync(
        int candidacyId, string completedStage, int userId, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyService.GetByIdAsync(candidacyId, cancellationToken);
        var workflow = await _workflowEngine.GetWorkflowDefinitionAsync(candidacy.OrgUnitId, cancellationToken);

        if (workflow == null)
            return ScreeningStageResult.NoWorkflowDefined();

        var stages = GetActiveStages(workflow);
        var nextStage = GetNextStage(stages, completedStage);

        if (nextStage == null)
        {
            // No more stages - screening process complete
            return ScreeningStageResult.Success(completedStage, null);
        }

        // Map stage to status code for transition
        var targetStatusCode = MapStageToStatusCode(nextStage);

        // Check if transition is allowed
        if (!await _workflowEngine.CanTransitionAsync(candidacyId, targetStatusCode, cancellationToken))
        {
            return ScreeningStageResult.Failed(
                $"מעבר מ-'{completedStage}' ל-'{nextStage}' אינו מותר בתהליך המיון");
        }

        // Execute the transition
        var transitionResult = await _workflowEngine.ExecuteTransitionAsync(
            candidacyId, targetStatusCode, $"מעבר אוטומטי לאחר השלמת שלב {completedStage}", userId, cancellationToken);

        if (!transitionResult.IsSuccess)
        {
            return ScreeningStageResult.Failed(
                transitionResult.ErrorMessage ?? "שגיאה במעבר סטטוס");
        }

        // Send notification for the status change
        await SendStageTransitionNotificationAsync(
            candidacy.OrgUnitId, candidacyId, completedStage, nextStage, cancellationToken);

        return ScreeningStageResult.Success(completedStage, nextStage);
    }

    public async Task<ScreeningStageResult> InitiateCandidacyScreeningAsync(
        int candidacyId, int userId, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyService.GetByIdAsync(candidacyId, cancellationToken);
        var workflow = await _workflowEngine.GetWorkflowDefinitionAsync(candidacy.OrgUnitId, cancellationToken);

        if (workflow == null)
            return ScreeningStageResult.NoWorkflowDefined();

        var stages = GetActiveStages(workflow);
        if (!stages.Any())
            return ScreeningStageResult.Failed("לא הוגדרו שלבי מיון פעילים");

        // If threshold check is the first stage, run it
        if (workflow.ThresholdCheckEnabled && stages.First() == StageThresholdCheck)
        {
            var thresholdResult = await _thresholdCheckService.CheckAllAsync(candidacyId, cancellationToken);

            if (!thresholdResult.AllPassed)
            {
                // Notify candidate about threshold failure
                await SendStageTransitionNotificationAsync(
                    candidacy.OrgUnitId, candidacyId, StageThresholdCheck, null, cancellationToken);

                return ScreeningStageResult.ThresholdFailed(StageThresholdCheck);
            }

            // Threshold passed - move to next stage
            var nextAfterThreshold = GetNextStage(stages, StageThresholdCheck);
            if (nextAfterThreshold != null)
            {
                var targetStatusCode = MapStageToStatusCode(nextAfterThreshold);

                if (await _workflowEngine.CanTransitionAsync(candidacyId, targetStatusCode, cancellationToken))
                {
                    var transitionResult = await _workflowEngine.ExecuteTransitionAsync(
                        candidacyId, targetStatusCode,
                        "מעבר אוטומטי לאחר עמידה בתנאי סף", userId, cancellationToken);

                    if (transitionResult.IsSuccess)
                    {
                        await SendStageTransitionNotificationAsync(
                            candidacy.OrgUnitId, candidacyId, StageThresholdCheck, nextAfterThreshold, cancellationToken);

                        return ScreeningStageResult.Success(StageThresholdCheck, nextAfterThreshold);
                    }
                }
            }

            return ScreeningStageResult.Success(StageThresholdCheck, nextAfterThreshold);
        }

        // No threshold check - move directly to first stage
        var firstStage = stages.First();
        var firstStageStatusCode = MapStageToStatusCode(firstStage);

        if (await _workflowEngine.CanTransitionAsync(candidacyId, firstStageStatusCode, cancellationToken))
        {
            var transitionResult = await _workflowEngine.ExecuteTransitionAsync(
                candidacyId, firstStageStatusCode,
                "התחלת תהליך מיון", userId, cancellationToken);

            if (transitionResult.IsSuccess)
            {
                await SendStageTransitionNotificationAsync(
                    candidacy.OrgUnitId, candidacyId, "initiated", firstStage, cancellationToken);

                return ScreeningStageResult.Success("initiated", firstStage);
            }

            return ScreeningStageResult.Failed(
                transitionResult.ErrorMessage ?? "שגיאה בהתחלת תהליך מיון");
        }

        return ScreeningStageResult.Failed("לא ניתן להתחיל את תהליך המיון - מעבר סטטוס אינו מותר");
    }

    /// <summary>
    /// Returns the ordered list of active screening stages based on workflow config
    /// </summary>
    internal static List<string> GetActiveStages(WorkflowDefinitionDto workflow)
    {
        var allStages = new List<(string stage, bool enabled)>
        {
            (StageThresholdCheck, workflow.ThresholdCheckEnabled),
            (StageExam, workflow.ExamStepEnabled),
            (StageInterview, workflow.InterviewStepEnabled),
            (StageCommittee, workflow.CommitteeStepEnabled)
        };

        // If StepOrder is defined, use it to reorder
        if (!string.IsNullOrWhiteSpace(workflow.StepOrder))
        {
            var orderedSteps = workflow.StepOrder.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLowerInvariant())
                .ToList();

            return orderedSteps
                .Where(step => allStages.Any(s => s.stage == step && s.enabled))
                .ToList();
        }

        // Default order
        return allStages.Where(s => s.enabled).Select(s => s.stage).ToList();
    }

    /// <summary>
    /// Gets the next stage after the completed stage
    /// </summary>
    internal static string? GetNextStage(List<string> stages, string completedStage)
    {
        var index = stages.IndexOf(completedStage);
        if (index < 0 || index >= stages.Count - 1)
            return null;
        return stages[index + 1];
    }

    /// <summary>
    /// Maps a screening stage name to a status code for workflow transitions
    /// </summary>
    internal static string MapStageToStatusCode(string stage) => stage switch
    {
        StageThresholdCheck => "threshold_check_pending",
        StageExam => "exam_pending",
        StageInterview => "interview_pending",
        StageCommittee => "committee_pending",
        _ => stage
    };

    private async Task SendStageTransitionNotificationAsync(
        int orgUnitId, int candidacyId, string fromStage, string? toStage,
        CancellationToken cancellationToken)
    {
        var variables = new Dictionary<string, string>
        {
            ["from_stage"] = fromStage,
            ["to_stage"] = toStage ?? "completed"
        };

        await _notificationService.TriggerAsync(
            orgUnitId, candidacyId, TriggerEventType.StatusChange, variables, cancellationToken);
    }
}
