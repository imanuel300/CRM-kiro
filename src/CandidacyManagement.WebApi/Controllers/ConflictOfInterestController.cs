using CandidacyManagement.Application.ConflictsOfInterest;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/conflicts-of-interest")]
public class ConflictOfInterestController : ControllerBase
{
    private readonly IConflictOfInterestService _service;

    public ConflictOfInterestController(IConflictOfInterestService service)
    {
        _service = service;
    }

    // --- ניגוד עניינים ---

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConflictOfInterestDto>> GetConflict(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetConflictByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ConflictOfInterestDto>> CreateConflict(
        [FromBody] CreateConflictOfInterestCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateConflictAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetConflict), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ConflictOfInterestDto>> UpdateConflict(
        int id, [FromBody] UpdateConflictOfInterestCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command Id" });

        var result = await _service.UpdateConflictAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteConflict(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteConflictAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/review")]
    public async Task<ActionResult<ConflictOfInterestDto>> ReviewConflict(
        int id, [FromBody] ReviewConflictCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command Id" });

        var result = await _service.ReviewConflictAsync(command, cancellationToken);
        return Ok(result);
    }

    // --- קרבה משפחתית ---

    [HttpGet("family-relations/{id:int}")]
    public async Task<ActionResult<FamilyRelationDto>> GetFamilyRelation(
        int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetFamilyRelationByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("family-relations")]
    public async Task<ActionResult<FamilyRelationDto>> CreateFamilyRelation(
        [FromBody] CreateFamilyRelationCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateFamilyRelationAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetFamilyRelation), new { id = result.Id }, result);
    }

    [HttpPut("family-relations/{id:int}")]
    public async Task<ActionResult<FamilyRelationDto>> UpdateFamilyRelation(
        int id, [FromBody] UpdateFamilyRelationCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "Route id does not match command Id" });

        var result = await _service.UpdateFamilyRelationAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("family-relations/{id:int}")]
    public async Task<IActionResult> DeleteFamilyRelation(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteFamilyRelationAsync(id, cancellationToken);
        return NoContent();
    }

    // --- הצהרות למועמדות ---

    [HttpGet("candidacy/{candidacyId:int}")]
    public async Task<ActionResult<CandidacyDeclarationsDto>> GetDeclarationsForCandidacy(
        int candidacyId, CancellationToken cancellationToken)
    {
        var result = await _service.GetDeclarationsForCandidacyAsync(candidacyId, cancellationToken);
        return Ok(result);
    }

    // --- מועמדויות הדורשות בדיקה ידנית ---

    [HttpGet("manual-review")]
    public async Task<ActionResult<IEnumerable<int>>> GetCandidaciesRequiringManualReview(
        [FromQuery] int? orgUnitId, CancellationToken cancellationToken)
    {
        var query = new ManualReviewQueryParams(orgUnitId);
        var result = await _service.GetCandidacyIdsRequiringManualReviewAsync(query, cancellationToken);
        return Ok(result);
    }
}
