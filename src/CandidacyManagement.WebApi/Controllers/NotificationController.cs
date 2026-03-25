using CandidacyManagement.Application.Notifications;
using CandidacyManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly IBulkNotificationService _bulkService;

    public NotificationController(INotificationService service, IBulkNotificationService bulkService)
    {
        _service = service;
        _bulkService = bulkService;
    }

    // --- ניהול תבניות ---

    [HttpGet("templates")]
    public async Task<ActionResult<IEnumerable<NotificationTemplateDto>>> ListTemplates(
        [FromQuery] int? orgUnitId,
        [FromQuery] TriggerEventType? triggerEvent,
        CancellationToken cancellationToken)
    {
        var query = new NotificationTemplateQueryParams(orgUnitId, triggerEvent);
        var result = await _service.ListTemplatesAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("templates/{id:int}")]
    public async Task<ActionResult<NotificationTemplateDto>> GetTemplate(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetTemplateByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("templates")]
    public async Task<ActionResult<NotificationTemplateDto>> CreateTemplate(
        [FromBody] CreateNotificationTemplateCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateTemplateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetTemplate), new { id = result.Id }, result);
    }

    [HttpPut("templates/{id:int}")]
    public async Task<ActionResult<NotificationTemplateDto>> UpdateTemplate(
        int id, [FromBody] UpdateNotificationTemplateCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command Id" });

        var result = await _service.UpdateTemplateAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("templates/{id:int}")]
    public async Task<IActionResult> DeleteTemplate(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteTemplateAsync(id, cancellationToken);
        return NoContent();
    }

    // --- שליחה ידנית ---

    [HttpPost("send")]
    public async Task<ActionResult<NotificationLogDto>> Send(
        [FromBody] SendNotificationCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.SendManualAsync(command, cancellationToken);
        return Ok(result);
    }

    // --- שליחה מרובה ---

    [HttpPost("send-bulk")]
    public async Task<ActionResult<BulkNotificationResultDto>> SendBulk(
        [FromBody] BulkSendNotificationCommand command, CancellationToken cancellationToken)
    {
        var result = await _bulkService.SendBulkAsync(command, cancellationToken);
        return Ok(result);
    }

    // --- יומן הודעות ---

    [HttpGet("logs")]
    public async Task<ActionResult<IEnumerable<NotificationLogDto>>> GetLogs(
        [FromQuery] int? candidacyId,
        [FromQuery] int? orgUnitId,
        CancellationToken cancellationToken)
    {
        var query = new NotificationLogQueryParams(candidacyId, orgUnitId);
        var result = await _service.GetLogsAsync(query, cancellationToken);
        return Ok(result);
    }
}
