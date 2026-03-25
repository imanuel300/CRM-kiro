namespace CandidacyManagement.Application.Dashboard;

// --- Parameters ---

public record DashboardParams(
    int OrgUnitId);

// --- Results ---

public record DashboardDataDto(
    int OrgUnitId,
    string OrgUnitName,
    int ActiveCandidacies,
    IEnumerable<StageBreakdownDto> ByScreeningStage,
    int CandidaciesRequiringAttention,
    IEnumerable<AttentionItemDto> AttentionItems);

public record StageBreakdownDto(
    string StageCategory,
    string StageDisplayName,
    int Count);

public record AttentionItemDto(
    int CandidacyId,
    int ContactId,
    string StatusCode,
    string StatusDisplayName,
    DateTime LastUpdated,
    string Reason);

public record OrgUnitDashboardSummaryDto(
    int OrgUnitId,
    string OrgUnitName,
    int ActiveCandidacies,
    int CandidaciesRequiringAttention,
    IEnumerable<StageBreakdownDto> ByScreeningStage);
