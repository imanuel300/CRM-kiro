using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.Tenures;

public record TenureDto(
    int Id,
    int ContactId,
    int OrgUnitId,
    string Position,
    DateTime StartDate,
    DateTime ExpectedEndDate,
    DateTime? ActualEndDate,
    TenureEndReason? EndReason,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt);

public record CreateTenureCommand(
    int ContactId,
    int OrgUnitId,
    string Position,
    DateTime StartDate,
    DateTime ExpectedEndDate,
    string? Notes);

public record UpdateTenureCommand(
    int Id,
    string Position,
    DateTime StartDate,
    DateTime ExpectedEndDate,
    string? Notes);

public record EndTenureCommand(
    int Id,
    TenureEndReason EndReason,
    DateTime? ActualEndDate,
    string? Notes);

public record TenureQueryParams(
    int? ContactId = null,
    int? OrgUnitId = null,
    bool? IsActive = null);

public record ExpiringTenureDto(
    int Id,
    int ContactId,
    int OrgUnitId,
    string Position,
    DateTime ExpectedEndDate,
    int DaysUntilExpiry);
