namespace CandidacyManagement.Application.CallsForCandidates;

public record CallForCandidatesDto(
    int Id,
    int OrgUnitId,
    string Title,
    string? Description,
    DateTime OpenDate,
    DateTime? CloseDate,
    bool IsTender,
    decimal? MinScore,
    string? EligibilityConditions,
    bool IsActive,
    DateTime CreatedAt);

public record CallForCandidatesDetailDto(
    int Id,
    int OrgUnitId,
    string Title,
    string? Description,
    DateTime OpenDate,
    DateTime? CloseDate,
    bool IsTender,
    decimal? MinScore,
    string? EligibilityConditions,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<ThresholdConditionDto> ThresholdConditions,
    IEnumerable<PositionDto> Positions);

public record ThresholdConditionDto(
    int Id,
    int CallForCandidatesId,
    string FieldName,
    string Operator,
    string Value,
    bool IsAutomatic);

public record PositionDto(
    int Id,
    int CallForCandidatesId,
    string Title,
    string? Description,
    int Vacancies);

public record CreateCallForCandidatesCommand(
    int OrgUnitId,
    string Title,
    string? Description,
    DateTime OpenDate,
    DateTime? CloseDate,
    bool IsTender,
    decimal? MinScore,
    string? EligibilityConditions);

public record UpdateCallForCandidatesCommand(
    int Id,
    string Title,
    string? Description,
    DateTime OpenDate,
    DateTime? CloseDate,
    bool IsTender,
    decimal? MinScore,
    string? EligibilityConditions);

public record CreateThresholdConditionCommand(
    int CallForCandidatesId,
    string FieldName,
    string Operator,
    string Value,
    bool IsAutomatic);

public record CreatePositionCommand(
    int CallForCandidatesId,
    string Title,
    string? Description,
    int Vacancies);

public record CallForCandidatesQueryParams(
    int? OrgUnitId = null,
    bool? IsActive = null,
    bool? IsTender = null);

public record ClosingSummaryDto(
    int CallForCandidatesId,
    string Title,
    DateTime? CloseDate,
    int TotalCandidacies,
    int MetThreshold,
    int Rejected);
