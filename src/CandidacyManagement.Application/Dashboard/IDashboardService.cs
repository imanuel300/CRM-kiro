namespace CandidacyManagement.Application.Dashboard;

public interface IDashboardService
{
    /// <summary>
    /// נתוני לוח מחוונים ברמת יחידה ארגונית: מועמדויות פעילות, לפי שלב מיון, דורשות טיפול
    /// </summary>
    Task<DashboardDataDto> GetDashboardDataAsync(
        int orgUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// נתוני לוח מחוונים מסוכמים לפי יחידה ארגונית (חוצה יחידות)
    /// </summary>
    Task<OrgUnitDashboardSummaryDto> GetDashboardByOrgUnitAsync(
        int orgUnitId, CancellationToken cancellationToken = default);
}
