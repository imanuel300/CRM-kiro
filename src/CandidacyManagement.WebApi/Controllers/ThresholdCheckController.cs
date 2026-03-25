using CandidacyManagement.Application.ThresholdChecks;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/candidacies/{candidacyId:int}/threshold-checks")]
public class ThresholdCheckController : ControllerBase
{
    private readonly IThresholdCheckService _service;

    public ThresholdCheckController(IThresholdCheckService service)
    {
        _service = service;
    }

    /// <summary>בדיקת כל תנאי הסף למועמדות</summary>
    [HttpPost("check-all")]
    public async Task<ActionResult<CheckAllResultDto>> CheckAll(
        int candidacyId, CancellationToken cancellationToken)
    {
        var result = await _service.CheckAllAsync(candidacyId, cancellationToken);
        return Ok(result);
    }

    /// <summary>בדיקת תנאי סף בודד</summary>
    [HttpPost("check/{conditionId:int}")]
    public async Task<ActionResult<ThresholdCheckResultDto>> CheckSingle(
        int candidacyId, int conditionId, CancellationToken cancellationToken)
    {
        var result = await _service.CheckSingleAsync(candidacyId, conditionId, cancellationToken);
        return Ok(result);
    }

    /// <summary>בדיקה ידנית לתנאי שאינו אוטומטי</summary>
    [HttpPost("manual")]
    public async Task<ActionResult<ThresholdCheckResultDto>> ManualCheck(
        int candidacyId, [FromBody] ManualCheckCommand command, CancellationToken cancellationToken)
    {
        if (candidacyId != command.CandidacyId)
            return BadRequest(new { message = "Route candidacyId does not match command CandidacyId" });

        var result = await _service.ManualCheckAsync(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>שליפת כל תוצאות הבדיקה למועמדות</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ThresholdCheckResultDto>>> GetResults(
        int candidacyId, CancellationToken cancellationToken)
    {
        var result = await _service.GetResultsAsync(candidacyId, cancellationToken);
        return Ok(result);
    }
}
