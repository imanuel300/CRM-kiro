using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.Workflow;

public record WorkflowDefinitionDto(
    int Id,
    int OrgUnitId,
    string Name,
    bool ExamStepEnabled,
    bool InterviewStepEnabled,
    bool CommitteeStepEnabled,
    bool ThresholdCheckEnabled,
    string? StepOrder,
    int Version,
    bool IsActive,
    DateTime CreatedAt);

public record StatusDefinitionDto(
    int Id,
    int OrgUnitId,
    string Code,
    string DisplayName,
    CandidacyStatusCategory Category,
    bool IsFinal,
    bool IsInitial,
    int SortOrder,
    IEnumerable<SubStatusDefinitionDto> SubStatuses);

public record SubStatusDefinitionDto(
    int Id,
    string Code,
    string DisplayName);

public record StatusTransitionDto(
    int Id,
    int FromStatusId,
    int ToStatusId,
    string FromStatusCode,
    string ToStatusCode,
    string? RequiredPermission,
    bool RequiresReason,
    string? AutoTriggerRule);

public record StatusTransitionResult(
    bool IsSuccess,
    string? StatusCode,
    string? ErrorMessage)
{
    public static StatusTransitionResult Success(string statusCode) => new(true, statusCode, null);
    public static StatusTransitionResult NotAllowed(string reason = "מעבר סטטוס זה אינו מותר") => new(false, null, reason);
    public static StatusTransitionResult Failed(string error) => new(false, null, error);
}
