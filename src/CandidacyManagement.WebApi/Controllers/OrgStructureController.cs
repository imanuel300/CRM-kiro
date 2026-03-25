using CandidacyManagement.Application.OrgStructure;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/org-structure")]
public class OrgStructureController : ControllerBase
{
    private readonly IOrgStructureService _service;

    public OrgStructureController(IOrgStructureService service)
    {
        _service = service;
    }

    // Sub-Unit endpoints

    [HttpPost("sub-units")]
    public async Task<ActionResult<OrgSubUnitDto>> CreateSubUnit(
        [FromBody] CreateSubUnitCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateSubUnitAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetSubUnitTree), new { orgUnitId = result.OrgUnitId }, result);
    }

    [HttpPut("sub-units/{id:int}")]
    public async Task<ActionResult<OrgSubUnitDto>> UpdateSubUnit(
        int id, [FromBody] UpdateSubUnitCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command id" });

        var result = await _service.UpdateSubUnitAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("sub-units/{id:int}")]
    public async Task<IActionResult> DeleteSubUnit(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteSubUnitAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("tree/{orgUnitId:int}")]
    public async Task<ActionResult<OrgSubUnitTreeDto>> GetSubUnitTree(
        int orgUnitId, CancellationToken cancellationToken)
    {
        var result = await _service.GetSubUnitTreeAsync(orgUnitId, cancellationToken);
        return Ok(result);
    }

    // Position endpoints

    [HttpPost("positions")]
    public async Task<ActionResult<OrgPositionDto>> CreatePosition(
        [FromBody] CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreatePositionAsync(command, cancellationToken);
        return Created(string.Empty, result);
    }

    [HttpPut("positions/{id:int}")]
    public async Task<ActionResult<OrgPositionDto>> UpdatePosition(
        int id, [FromBody] UpdatePositionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command id" });

        var result = await _service.UpdatePositionAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("positions/{id:int}")]
    public async Task<IActionResult> DeletePosition(int id, CancellationToken cancellationToken)
    {
        await _service.DeletePositionAsync(id, cancellationToken);
        return NoContent();
    }

    // Assignment endpoints

    [HttpPost("assign")]
    public async Task<ActionResult<PositionAssignmentDto>> AssignToPosition(
        [FromBody] AssignToPositionCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.AssignToPositionAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("unassign")]
    public async Task<IActionResult> UnassignFromPosition(
        [FromBody] UnassignFromPositionCommand command, CancellationToken cancellationToken)
    {
        await _service.UnassignFromPositionAsync(command, cancellationToken);
        return NoContent();
    }

    // Occupancy endpoint

    [HttpGet("occupancy/{subUnitId:int}")]
    public async Task<ActionResult<SubUnitOccupancyDto>> GetPositionOccupancy(
        int subUnitId, CancellationToken cancellationToken)
    {
        var result = await _service.GetPositionOccupancyAsync(subUnitId, cancellationToken);
        return Ok(result);
    }
}
