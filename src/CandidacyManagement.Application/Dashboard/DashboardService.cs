using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IRepository<Candidacy> _candidacyRepo;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepo;
    private readonly IRepository<StatusDefinition> _statusRepo;
    private readonly IRepository<CandidacyStatusHistory> _historyRepo;

    /// <summary>
    /// Number of days without a status change after which a candidacy is considered requiring attention.
    /// </summary>
    private const int StaleThresholdDays = 14;

    public DashboardService(
        IRepository<Candidacy> candidacyRepo,
        IRepository<OrganizationalUnit> orgUnitRepo,
        IRepository<StatusDefinition> statusRepo,
        IRepository<CandidacyStatusHistory> historyRepo)
    {
        _candidacyRepo = candidacyRepo;
        _orgUnitRepo = orgUnitRepo;
        _statusRepo = statusRepo;
        _historyRepo = historyRepo;
    }

    public async Task<DashboardDataDto> GetDashboardDataAsync(
        int orgUnitId, CancellationToken cancellationToken = default)
    {
        var orgUnit = await _orgUnitRepo.GetByIdAsync(orgUnitId, cancellationToken)
            ?? throw new NotFoundException("OrganizationalUnit", orgUnitId);

        var candidacies = await _candidacyRepo.FindAsync(
            c => c.OrgUnitId == orgUnitId && c.IsActive, cancellationToken);
        var candidacyList = candidacies.ToList();

        var statuses = await _statusRepo.FindAsync(
            s => s.OrgUnitId == orgUnitId, cancellationToken);
        var statusLookup = statuses.ToDictionary(s => s.Id, s => s);

        // Breakdown by screening stage (status category)
        var byStage = BuildStageBreakdown(candidacyList, statusLookup);

        // Candidacies requiring attention: active candidacies with no status change for StaleThresholdDays
        var histories = await _historyRepo.FindAsync(
            h => candidacyList.Select(c => c.Id).Contains(h.CandidacyId), cancellationToken);
        var historyList = histories.ToList();

        var attentionItems = BuildAttentionItems(candidacyList, statusLookup, historyList);

        return new DashboardDataDto(
            OrgUnitId: orgUnitId,
            OrgUnitName: orgUnit.Name,
            ActiveCandidacies: candidacyList.Count,
            ByScreeningStage: byStage,
            CandidaciesRequiringAttention: attentionItems.Count,
            AttentionItems: attentionItems);
    }

    public async Task<OrgUnitDashboardSummaryDto> GetDashboardByOrgUnitAsync(
        int orgUnitId, CancellationToken cancellationToken = default)
    {
        var orgUnit = await _orgUnitRepo.GetByIdAsync(orgUnitId, cancellationToken)
            ?? throw new NotFoundException("OrganizationalUnit", orgUnitId);

        var candidacies = await _candidacyRepo.FindAsync(
            c => c.OrgUnitId == orgUnitId && c.IsActive, cancellationToken);
        var candidacyList = candidacies.ToList();

        var statuses = await _statusRepo.FindAsync(
            s => s.OrgUnitId == orgUnitId, cancellationToken);
        var statusLookup = statuses.ToDictionary(s => s.Id, s => s);

        var byStage = BuildStageBreakdown(candidacyList, statusLookup);

        var histories = await _historyRepo.FindAsync(
            h => candidacyList.Select(c => c.Id).Contains(h.CandidacyId), cancellationToken);
        var historyList = histories.ToList();

        var attentionCount = CountAttentionItems(candidacyList, historyList);

        return new OrgUnitDashboardSummaryDto(
            OrgUnitId: orgUnitId,
            OrgUnitName: orgUnit.Name,
            ActiveCandidacies: candidacyList.Count,
            CandidaciesRequiringAttention: attentionCount,
            ByScreeningStage: byStage);
    }

    // --- Helpers ---

    private static List<StageBreakdownDto> BuildStageBreakdown(
        List<Candidacy> candidacies, Dictionary<int, StatusDefinition> statusLookup)
    {
        return candidacies
            .Where(c => c.CurrentStatusId.HasValue)
            .GroupBy(c =>
            {
                var status = statusLookup.GetValueOrDefault(c.CurrentStatusId!.Value);
                return status?.Category ?? CandidacyStatusCategory.Submitted;
            })
            .Select(g => new StageBreakdownDto(
                StageCategory: g.Key.ToString(),
                StageDisplayName: GetCategoryDisplayName(g.Key),
                Count: g.Count()))
            .OrderBy(s => s.StageCategory)
            .ToList();
    }

    private static List<AttentionItemDto> BuildAttentionItems(
        List<Candidacy> candidacies,
        Dictionary<int, StatusDefinition> statusLookup,
        List<CandidacyStatusHistory> histories)
    {
        var cutoff = DateTime.UtcNow.AddDays(-StaleThresholdDays);

        var latestChangePerCandidacy = histories
            .GroupBy(h => h.CandidacyId)
            .ToDictionary(g => g.Key, g => g.Max(h => h.ChangedAt));

        return candidacies
            .Where(c =>
            {
                if (latestChangePerCandidacy.TryGetValue(c.Id, out var lastChange))
                    return lastChange < cutoff;
                // No history at all — use CreatedAt
                return c.CreatedAt < cutoff;
            })
            .Select(c =>
            {
                var status = c.CurrentStatusId.HasValue
                    ? statusLookup.GetValueOrDefault(c.CurrentStatusId.Value)
                    : null;
                var lastUpdated = latestChangePerCandidacy.TryGetValue(c.Id, out var lc)
                    ? lc
                    : c.CreatedAt;

                return new AttentionItemDto(
                    CandidacyId: c.Id,
                    ContactId: c.ContactId,
                    StatusCode: status?.Code ?? "Unknown",
                    StatusDisplayName: status?.DisplayName ?? "Unknown",
                    LastUpdated: lastUpdated,
                    Reason: $"No status change for over {StaleThresholdDays} days");
            })
            .OrderBy(a => a.LastUpdated)
            .ToList();
    }

    private static int CountAttentionItems(
        List<Candidacy> candidacies,
        List<CandidacyStatusHistory> histories)
    {
        var cutoff = DateTime.UtcNow.AddDays(-StaleThresholdDays);

        var latestChangePerCandidacy = histories
            .GroupBy(h => h.CandidacyId)
            .ToDictionary(g => g.Key, g => g.Max(h => h.ChangedAt));

        return candidacies.Count(c =>
        {
            if (latestChangePerCandidacy.TryGetValue(c.Id, out var lastChange))
                return lastChange < cutoff;
            return c.CreatedAt < cutoff;
        });
    }

    private static string GetCategoryDisplayName(CandidacyStatusCategory category) =>
        category switch
        {
            CandidacyStatusCategory.Submitted => "הוגשה",
            CandidacyStatusCategory.InReview => "בבדיקה",
            CandidacyStatusCategory.Exam => "מבחן",
            CandidacyStatusCategory.Interview => "ראיון",
            CandidacyStatusCategory.Committee => "ועדה",
            CandidacyStatusCategory.Accepted => "התקבל",
            CandidacyStatusCategory.Rejected => "נדחה",
            CandidacyStatusCategory.Withdrawn => "פרש",
            _ => category.ToString()
        };
}
