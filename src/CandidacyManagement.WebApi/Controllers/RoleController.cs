using CandidacyManagement.Application.Roles;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/roles")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _service;

    public RoleController(IRoleService service)
    {
        _service = service;
    }

    // --- ניהול תפקידים ---

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetByOrgUnit(
        [FromQuery] int orgUnitId, CancellationToken cancellationToken)
    {
        var result = await _service.GetRolesByOrgUnitAsync(orgUnitId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoleDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetRoleByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create(
        [FromBody] CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateRoleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoleDto>> Update(
        int id, [FromBody] UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command id" });

        var result = await _service.UpdateRoleAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteRoleAsync(id, cancellationToken);
        return NoContent();
    }

    // --- שיוך משתמשים ---

    [HttpPost("assignments")]
    public async Task<ActionResult<UserRoleDto>> AssignUser(
        [FromBody] AssignUserRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.AssignUserRoleAsync(command, cancellationToken);
        return Created($"api/roles/assignments/{result.Id}", result);
    }

    [HttpDelete("assignments/{id:int}")]
    public async Task<IActionResult> RemoveAssignment(int id, CancellationToken cancellationToken)
    {
        await _service.RemoveUserRoleAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("users/{userId:int}/roles")]
    public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUserRoles(
        int userId, CancellationToken cancellationToken)
    {
        var result = await _service.GetUserRolesAsync(userId, cancellationToken);
        return Ok(result);
    }

    // --- יומן ביקורת ---

    [HttpGet("audit-logs")]
    public async Task<ActionResult<IEnumerable<AuditLogEntryDto>>> GetAuditLogs(
        [FromQuery] int? userId,
        [FromQuery] int? orgUnitId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = new AuditLogQueryParams(userId, orgUnitId, fromDate, toDate);
        var result = await _service.GetAuditLogsAsync(query, cancellationToken);
        return Ok(result);
    }
}
