using CandidacyManagement.Application.Committees;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/committees")]
public class CommitteeController : ControllerBase
{
    private readonly ICommitteeService _service;

    public CommitteeController(ICommitteeService service)
    {
        _service = service;
    }

    // --- Committee CRUD ---

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommitteeDto>>> List(
        [FromQuery] int? orgUnitId, CancellationToken cancellationToken)
    {
        var query = new CommitteeQueryParams(orgUnitId);
        var result = await _service.ListAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommitteeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CommitteeDto>> Create(
        [FromBody] CreateCommitteeCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommitteeDto>> Update(
        int id, [FromBody] UpdateCommitteeCommand command, CancellationToken cancellationToken)
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

    // --- Meetings ---

    [HttpPost("{id:int}/meetings")]
    public async Task<ActionResult<CommitteeMeetingDto>> CreateMeeting(
        int id, [FromBody] CreateMeetingCommand command, CancellationToken cancellationToken)
    {
        if (id != command.CommitteeId)
            return BadRequest(new { message = "Route id does not match command CommitteeId" });

        var result = await _service.CreateMeetingAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetMeeting), new { committeeId = id, meetingId = result.Id }, result);
    }

    [HttpGet("{id:int}/meetings")]
    public async Task<ActionResult<IEnumerable<CommitteeMeetingDto>>> ListMeetings(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.ListMeetingsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{committeeId:int}/meetings/{meetingId:int}")]
    public async Task<ActionResult<CommitteeMeetingDto>> GetMeeting(
        int committeeId, int meetingId, CancellationToken cancellationToken)
    {
        var result = await _service.GetMeetingAsync(meetingId, cancellationToken);
        return Ok(result);
    }

    // --- Decisions ---

    [HttpPost("{committeeId:int}/meetings/{meetingId:int}/decisions")]
    public async Task<ActionResult<CommitteeDecisionDto>> RecordDecision(
        int committeeId, int meetingId,
        [FromBody] RecordDecisionCommand command, CancellationToken cancellationToken)
    {
        if (meetingId != command.MeetingId)
            return BadRequest(new { message = "Route meetingId does not match command MeetingId" });

        var result = await _service.RecordDecisionAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{committeeId:int}/meetings/{meetingId:int}/decisions")]
    public async Task<ActionResult<IEnumerable<CommitteeDecisionDto>>> GetDecisions(
        int committeeId, int meetingId, CancellationToken cancellationToken)
    {
        var result = await _service.GetDecisionsAsync(meetingId, cancellationToken);
        return Ok(result);
    }

    // --- Appeals ---

    [HttpPost("{committeeId:int}/meetings/{meetingId:int}/appeals")]
    public async Task<ActionResult<CommitteeAppealDto>> SubmitAppeal(
        int committeeId, int meetingId,
        [FromBody] SubmitCommitteeAppealCommand command, CancellationToken cancellationToken)
    {
        if (meetingId != command.MeetingId)
            return BadRequest(new { message = "Route meetingId does not match command MeetingId" });

        var result = await _service.SubmitAppealAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{committeeId:int}/meetings/{meetingId:int}/appeals/{appealId:int}")]
    public async Task<ActionResult<CommitteeAppealDto>> ResolveAppeal(
        int committeeId, int meetingId, int appealId,
        [FromBody] ResolveCommitteeAppealCommand command, CancellationToken cancellationToken)
    {
        if (appealId != command.AppealId)
            return BadRequest(new { message = "Route appealId does not match command AppealId" });

        var result = await _service.ResolveAppealAsync(appealId, command.Result, cancellationToken);
        return Ok(result);
    }

    // --- Protocol ---

    [HttpGet("{committeeId:int}/meetings/{meetingId:int}/protocol")]
    public async Task<ActionResult<string>> GetProtocol(
        int committeeId, int meetingId, CancellationToken cancellationToken)
    {
        var result = await _service.GenerateProtocolAsync(meetingId, cancellationToken);
        return Content(result, "text/html");
    }
}
