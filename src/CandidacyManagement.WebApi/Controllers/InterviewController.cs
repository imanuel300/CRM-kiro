using CandidacyManagement.Application.Interviews;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/interviews")]
public class InterviewController : ControllerBase
{
    private readonly IInterviewService _service;

    public InterviewController(IInterviewService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InterviewDto>>> List(
        [FromQuery] int? orgUnitId,
        [FromQuery] int? callForCandidatesId,
        [FromQuery] int? candidacyId,
        CancellationToken cancellationToken)
    {
        var query = new InterviewQueryParams(orgUnitId, callForCandidatesId, candidacyId);
        var result = await _service.ListAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InterviewDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<InterviewDto>> Create(
        [FromBody] CreateInterviewCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InterviewDto>> Update(
        int id, [FromBody] UpdateInterviewCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command Id" });

        var result = await _service.UpdateAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // --- Feedback ---

    [HttpPost("{id:int}/feedback")]
    public async Task<ActionResult<InterviewFeedbackDto>> SubmitFeedback(
        int id, [FromBody] SubmitFeedbackCommand command, CancellationToken cancellationToken)
    {
        if (id != command.InterviewId)
            return BadRequest(new { message = "Route id does not match command InterviewId" });

        var result = await _service.SubmitFeedbackAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/feedback")]
    public async Task<ActionResult<IEnumerable<InterviewFeedbackDto>>> GetFeedback(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetFeedbackAsync(id, cancellationToken);
        return Ok(result);
    }

    // --- Second Interview ---

    [HttpPost("{id:int}/second-interview")]
    public async Task<ActionResult<InterviewDto>> ScheduleSecondInterview(
        int id, [FromBody] CreateInterviewCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.ScheduleSecondInterviewAsync(id, command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
