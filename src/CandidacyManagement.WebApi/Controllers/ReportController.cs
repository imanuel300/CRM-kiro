using CandidacyManagement.Application.Reports;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// דוח סטטוס מועמדויות ברמת יחידה ארגונית
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<StatusReportDto>> GetStatusReport(
        [FromQuery] int orgUnitId,
        [FromQuery] int? callForCandidatesId,
        [FromQuery] string? statusCode,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var parameters = new StatusReportParams(
            orgUnitId, callForCandidatesId, statusCode, fromDate, toDate);
        var result = await _reportService.GetCandidacyStatusReportAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// דוח מאוחד חוצה יחידות ארגוניות
    /// </summary>
    [HttpGet("cross-unit")]
    public async Task<ActionResult<CrossUnitReportDto>> GetCrossUnitReport(
        [FromQuery] int[]? orgUnitIds,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var parameters = new CrossUnitReportParams(orgUnitIds, fromDate, toDate);
        var result = await _reportService.GetCrossUnitReportAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// ייצוא דוח לפורמט Excel
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportToExcel(
        [FromQuery] string reportType,
        [FromQuery] int? orgUnitId,
        [FromQuery] int? callForCandidatesId,
        [FromQuery] string? statusCode,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int[]? orgUnitIds,
        CancellationToken cancellationToken)
    {
        var parameters = new ExportReportParams(
            reportType, orgUnitId, callForCandidatesId, statusCode, fromDate, toDate, orgUnitIds);
        var bytes = await _reportService.ExportToExcelAsync(parameters, cancellationToken);
        return File(bytes, "text/csv", $"report-{reportType}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    /// <summary>
    /// הפקת דוח מותאם אישית
    /// </summary>
    [HttpGet("custom")]
    public async Task<ActionResult<ReportResult>> GetCustomReport(
        [FromQuery] int orgUnitId,
        [FromQuery] int customReportDefinitionId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var parameters = new CustomReportParams(
            orgUnitId, customReportDefinitionId, fromDate, toDate);
        var result = await _reportService.GetCustomReportAsync(parameters, cancellationToken);
        return Ok(result);
    }

    // --- Custom Report Definition Management ---

    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<CustomReportDefinitionDto>>> GetDefinitions(
        [FromQuery] int orgUnitId, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetCustomReportDefinitionsAsync(orgUnitId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("definitions")]
    public async Task<ActionResult<CustomReportDefinitionDto>> CreateDefinition(
        [FromBody] CreateCustomReportDefinitionCommand command, CancellationToken cancellationToken)
    {
        var result = await _reportService.CreateCustomReportDefinitionAsync(command, cancellationToken);
        return Created($"api/reports/definitions/{result.Id}", result);
    }

    [HttpPut("definitions/{id:int}")]
    public async Task<ActionResult<CustomReportDefinitionDto>> UpdateDefinition(
        int id, [FromBody] UpdateCustomReportDefinitionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command id" });

        var result = await _reportService.UpdateCustomReportDefinitionAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("definitions/{id:int}")]
    public async Task<IActionResult> DeleteDefinition(int id, CancellationToken cancellationToken)
    {
        await _reportService.DeleteCustomReportDefinitionAsync(id, cancellationToken);
        return NoContent();
    }
}
