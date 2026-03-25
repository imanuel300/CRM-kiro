using CandidacyManagement.Application.Candidacies;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/candidacies")]
public class CandidacyController : ControllerBase
{
    private readonly ICandidacyService _service;

    public CandidacyController(ICandidacyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidacyDto>>> List(
        [FromQuery] int? orgUnitId,
        [FromQuery] int? contactId,
        [FromQuery] int? callForCandidatesId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = new CandidacyQueryParams(orgUnitId, contactId, callForCandidatesId, isActive);
        var result = await _service.ListAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CandidacyDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/detail")]
    public async Task<ActionResult<CandidacyDetailDto>> GetDetail(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CandidacyDto>> Create(
        [FromBody] CreateCandidacyCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CandidacyDto>> Update(
        int id, [FromBody] UpdateCandidacyCommand command, CancellationToken cancellationToken)
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

    [HttpGet("{id:int}/custom-fields")]
    public async Task<ActionResult<IEnumerable<CandidacyCustomFieldValueDto>>> GetCustomFields(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetCustomFieldsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}/custom-fields")]
    public async Task<IActionResult> SetCustomFieldValue(
        int id, [FromBody] SetCandidacyCustomFieldValueCommand command, CancellationToken cancellationToken)
    {
        if (id != command.CandidacyId)
            return BadRequest(new { message = "Route id does not match command CandidacyId" });

        await _service.SetCustomFieldValueAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/transition")]
    public async Task<ActionResult<CandidacyDto>> TransitionStatus(
        int id, [FromBody] TransitionStatusCommand command, CancellationToken cancellationToken)
    {
        if (id != command.CandidacyId)
            return BadRequest(new { message = "Route id does not match command CandidacyId" });

        var result = await _service.TransitionStatusAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/status-history")]
    public async Task<ActionResult<IEnumerable<StatusHistoryDto>>> GetStatusHistory(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetStatusHistoryAsync(id, cancellationToken);
        return Ok(result);
    }
}
