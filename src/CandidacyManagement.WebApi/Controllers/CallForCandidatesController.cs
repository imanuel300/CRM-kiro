using CandidacyManagement.Application.CallsForCandidates;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/calls-for-candidates")]
public class CallForCandidatesController : ControllerBase
{
    private readonly ICallForCandidatesService _service;

    public CallForCandidatesController(ICallForCandidatesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CallForCandidatesDto>>> List(
        [FromQuery] int? orgUnitId,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isTender,
        CancellationToken cancellationToken)
    {
        var query = new CallForCandidatesQueryParams(orgUnitId, isActive, isTender);
        var result = await _service.ListAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CallForCandidatesDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/detail")]
    public async Task<ActionResult<CallForCandidatesDetailDto>> GetDetail(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CallForCandidatesDto>> Create(
        [FromBody] CreateCallForCandidatesCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CallForCandidatesDto>> Update(
        int id, [FromBody] UpdateCallForCandidatesCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command id" });

        var result = await _service.UpdateAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // --- Threshold Conditions ---

    [HttpGet("{id:int}/threshold-conditions")]
    public async Task<ActionResult<IEnumerable<ThresholdConditionDto>>> GetThresholdConditions(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetThresholdConditionsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/threshold-conditions")]
    public async Task<ActionResult<ThresholdConditionDto>> AddThresholdCondition(
        int id, [FromBody] CreateThresholdConditionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.CallForCandidatesId)
            return BadRequest(new { message = "Route id does not match command CallForCandidatesId" });

        var result = await _service.AddThresholdConditionAsync(command, cancellationToken);
        return Created($"api/calls-for-candidates/{id}/threshold-conditions/{result.Id}", result);
    }

    [HttpDelete("{id:int}/threshold-conditions/{conditionId:int}")]
    public async Task<IActionResult> RemoveThresholdCondition(int id, int conditionId, CancellationToken cancellationToken)
    {
        await _service.RemoveThresholdConditionAsync(conditionId, cancellationToken);
        return NoContent();
    }

    // --- Positions ---

    [HttpGet("{id:int}/positions")]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetPositions(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetPositionsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/positions")]
    public async Task<ActionResult<PositionDto>> AddPosition(
        int id, [FromBody] CreatePositionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.CallForCandidatesId)
            return BadRequest(new { message = "Route id does not match command CallForCandidatesId" });

        var result = await _service.AddPositionAsync(command, cancellationToken);
        return Created($"api/calls-for-candidates/{id}/positions/{result.Id}", result);
    }

    [HttpDelete("{id:int}/positions/{positionId:int}")]
    public async Task<IActionResult> RemovePosition(int id, int positionId, CancellationToken cancellationToken)
    {
        await _service.RemovePositionAsync(positionId, cancellationToken);
        return NoContent();
    }

    // --- Closing Logic ---

    [HttpGet("{id:int}/is-closed")]
    public async Task<ActionResult<bool>> IsClosed(int id, CancellationToken cancellationToken)
    {
        var result = await _service.IsClosedAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/closing-summary")]
    public async Task<ActionResult<ClosingSummaryDto>> GetClosingSummary(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetClosingSummaryAsync(id, cancellationToken);
        return Ok(result);
    }
}
