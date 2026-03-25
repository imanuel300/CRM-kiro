using CandidacyManagement.Application.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// נתוני לוח מחוונים ברמת יחידה ארגונית
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DashboardDataDto>> GetDashboard(
        [FromQuery] int orgUnitId,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetDashboardDataAsync(orgUnitId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// נתוני לוח מחוונים מסוכמים ליחידה ארגונית ספציפית
    /// </summary>
    [HttpGet("org-unit/{orgUnitId:int}")]
    public async Task<ActionResult<OrgUnitDashboardSummaryDto>> GetDashboardByOrgUnit(
        int orgUnitId,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetDashboardByOrgUnitAsync(orgUnitId, cancellationToken);
        return Ok(result);
    }
}
