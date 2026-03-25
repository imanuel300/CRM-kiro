using CandidacyManagement.Application.Exams;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/exams")]
public class ExamController : ControllerBase
{
    private readonly IExamService _service;

    public ExamController(IExamService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExamDto>>> List(
        [FromQuery] int? orgUnitId,
        [FromQuery] int? callForCandidatesId,
        CancellationToken cancellationToken)
    {
        var query = new ExamQueryParams(orgUnitId, callForCandidatesId);
        var result = await _service.ListAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExamDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExamDto>> Create(
        [FromBody] CreateExamCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExamDto>> Update(
        int id, [FromBody] UpdateExamCommand command, CancellationToken cancellationToken)
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

    // --- Scores ---

    [HttpPost("{id:int}/scores")]
    public async Task<ActionResult<ExamScoreDto>> SubmitScore(
        int id, [FromBody] SubmitScoreCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ExamId)
            return BadRequest(new { message = "Route id does not match command ExamId" });

        var result = await _service.SubmitScoreAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/scores")]
    public async Task<ActionResult<IEnumerable<ExamScoreDto>>> GetScores(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetScoresByExamAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{examId:int}/scores/{candidacyId:int}")]
    public async Task<ActionResult<ExamScoreDto>> GetScore(
        int examId, int candidacyId, CancellationToken cancellationToken)
    {
        var result = await _service.GetScoreAsync(examId, candidacyId, cancellationToken);
        return Ok(result);
    }

    // --- Appeals ---

    [HttpPost("{id:int}/appeals")]
    public async Task<ActionResult<ExamScoreDto>> SubmitAppeal(
        int id, [FromBody] SubmitAppealCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ExamId)
            return BadRequest(new { message = "Route id does not match command ExamId" });

        var result = await _service.SubmitAppealAsync(command, cancellationToken);
        return Ok(result);
    }
}
