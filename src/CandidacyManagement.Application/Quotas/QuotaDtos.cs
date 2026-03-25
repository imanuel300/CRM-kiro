namespace CandidacyManagement.Application.Quotas;

public record QuotaDto(
    int Id,
    int OrgUnitId,
    string CategoryName,
    int TargetCount,
    int CurrentCount,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);

public record CreateQuotaCommand(
    int OrgUnitId,
    string CategoryName,
    int TargetCount,
    string? Description);

public record UpdateQuotaCommand(
    int Id,
    string CategoryName,
    int TargetCount,
    string? Description,
    bool IsActive);

public record AssignCandidacyCommand(
    int QuotaId,
    int CandidacyId);

public record UnassignCandidacyCommand(
    int QuotaId,
    int CandidacyId);

public record QuotaAssignmentDto(
    int Id,
    int QuotaId,
    int CandidacyId,
    DateTime CreatedAt);

public record QuotaFulfillmentDto(
    int QuotaId,
    string CategoryName,
    int TargetCount,
    int CurrentCount,
    double FulfillmentPercentage,
    bool IsActive);

public record OrgUnitFulfillmentDto(
    int OrgUnitId,
    IEnumerable<QuotaFulfillmentDto> Quotas);
