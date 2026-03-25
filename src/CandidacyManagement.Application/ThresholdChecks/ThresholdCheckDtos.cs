using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.ThresholdChecks;

public record ThresholdCheckResultDto(
    int Id,
    int CandidacyId,
    int ThresholdConditionId,
    string FieldName,
    ConditionType ConditionType,
    bool Passed,
    string? ActualValue,
    string? Notes,
    bool IsAutomatic,
    int? CheckedByUserId,
    DateTime CheckedAt);

public record CheckAllResultDto(
    int CandidacyId,
    bool AllPassed,
    IEnumerable<ThresholdCheckResultDto> Results);

public record ManualCheckCommand(
    int CandidacyId,
    int ConditionId,
    bool Passed,
    string? Notes,
    int UserId);
