using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.Workflow;

public record ConfigureWorkflowCommand(
    int OrgUnitId,
    string Name,
    bool ExamStepEnabled,
    bool InterviewStepEnabled,
    bool CommitteeStepEnabled,
    bool ThresholdCheckEnabled,
    string? StepOrder);

public record ConfigureStatusDefinition(
    string Code,
    string DisplayName,
    CandidacyStatusCategory Category,
    bool IsFinal,
    bool IsInitial,
    int SortOrder,
    IEnumerable<ConfigureSubStatusDefinition>? SubStatuses);

public record ConfigureSubStatusDefinition(
    string Code,
    string DisplayName);

public record ConfigureStatusesCommand(
    int OrgUnitId,
    IEnumerable<ConfigureStatusDefinition> Statuses);

public record ConfigureTransitionDefinition(
    string FromStatusCode,
    string ToStatusCode,
    string? RequiredPermission,
    bool RequiresReason,
    string? AutoTriggerRule);

public record ConfigureTransitionsCommand(
    int OrgUnitId,
    IEnumerable<ConfigureTransitionDefinition> Transitions);
