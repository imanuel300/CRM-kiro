using CandidacyManagement.Application.Contacts;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/contacts")]
public class ContactController : ControllerBase
{
    private readonly IContactService _service;

    public ContactController(IContactService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContactDto>>> Search(
        [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await _service.SearchAsync(search, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ContactDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-id-number/{idNumber}")]
    public async Task<ActionResult<ContactDto>> GetByIdNumber(string idNumber, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdNumberAsync(idNumber, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ContactDto>> Create(
        [FromBody] CreateContactCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ContactDto>> Update(
        int id, [FromBody] UpdateContactCommand command, CancellationToken cancellationToken)
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

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<IEnumerable<ChangeHistoryDto>>> GetChangeHistory(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetChangeHistoryAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/custom-fields/{orgUnitId:int}")]
    public async Task<ActionResult<IEnumerable<CustomFieldValueDto>>> GetCustomFields(
        int id, int orgUnitId, CancellationToken cancellationToken)
    {
        var result = await _service.GetCustomFieldsAsync(id, orgUnitId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}/custom-fields")]
    public async Task<IActionResult> SetCustomFieldValue(
        int id, [FromBody] SetCustomFieldValueCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ContactId)
            return BadRequest(new { message = "Route id does not match command ContactId" });

        await _service.SetCustomFieldValueAsync(command, cancellationToken);
        return NoContent();
    }
}
