using CandidacyManagement.Application.Quotas;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/quotas")]
public class QuotaController : ControllerBase
{
    private readonly IQuotaService _service;

    public QuotaController(IQuotaService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuotaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-org-unit/{orgUnitId:int}")]
    public async Task<ActionResult<IEnumerable<QuotaDto>>> GetByOrgUnit(
        int orgUnitId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByOrgUnitAsync(orgUnitId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<QuotaDto>> Create(
        [FromBody] CreateQuotaCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<QuotaDto>> Update(
        int id, [FromBody] UpdateQuotaCommand command, CancellationToken cancellationToken)
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

    [HttpPost("assign-candidacy")]
    public async Task<ActionResult<QuotaAssignmentDto>> AssignCandidacy(
        [FromBody] AssignCandidacyCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.AssignCandidacyAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("unassign-candidacy")]
    public async Task<IActionResult> UnassignCandidacy(
        [FromBody] UnassignCandidacyCommand command, CancellationToken cancellationToken)
    {
        await _service.UnassignCandidacyAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("fulfillment/{orgUnitId:int}")]
    public async Task<ActionResult<OrgUnitFulfillmentDto>> GetFulfillmentStatus(
        int orgUnitId, CancellationToken cancellationToken)
    {
        var result = await _service.GetFulfillmentStatusAsync(orgUnitId, cancellationToken);
        return Ok(result);
    }
}
