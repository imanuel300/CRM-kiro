using CandidacyManagement.Application.Tenures;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/tenures")]
public class TenureController : ControllerBase
{
    private readonly ITenureService _service;

    public TenureController(ITenureService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TenureDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TenureDto>> Create(
        [FromBody] CreateTenureCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TenureDto>> Update(
        int id, [FromBody] UpdateTenureCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command id" });

        var result = await _service.UpdateAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/end")]
    public async Task<ActionResult<TenureDto>> EndTenure(
        int id, [FromBody] EndTenureCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command id" });

        var result = await _service.EndTenureAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("by-contact/{contactId:int}")]
    public async Task<ActionResult<IEnumerable<TenureDto>>> GetByContact(
        int contactId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByContactAsync(contactId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-org-unit/{orgUnitId:int}")]
    public async Task<ActionResult<IEnumerable<TenureDto>>> GetByOrgUnit(
        int orgUnitId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByOrgUnitAsync(orgUnitId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<IEnumerable<ExpiringTenureDto>>> GetExpiringTenures(
        [FromQuery] int daysThreshold = 30,
        [FromQuery] int? orgUnitId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetExpiringTenuresAsync(daysThreshold, orgUnitId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history/{contactId:int}")]
    public async Task<ActionResult<IEnumerable<TenureDto>>> GetHistory(
        int contactId, CancellationToken cancellationToken)
    {
        var result = await _service.GetHistoryAsync(contactId, cancellationToken);
        return Ok(result);
    }
}
