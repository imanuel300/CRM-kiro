using CandidacyManagement.Application.Documents;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _service;
    private readonly IDocumentMergeService _mergeService;

    public DocumentController(IDocumentService service, IDocumentMergeService mergeService)
    {
        _service = service;
        _mergeService = mergeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> List(
        [FromQuery] int? candidacyId,
        [FromQuery] string? documentType,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new DocumentQueryParams(candidacyId, documentType, status);
        var result = await _service.ListAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DocumentDto>> Upload(
        [FromBody] UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.UploadAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // --- Review (Approve / Reject) ---

    [HttpPost("{id:int}/review")]
    public async Task<ActionResult<DocumentDto>> Review(
        int id, [FromBody] ReviewDocumentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.DocumentId)
            return BadRequest(new { message = "Route id does not match command DocumentId" });

        var result = await _service.ReviewAsync(command, cancellationToken);
        return Ok(result);
    }

    // --- Version History ---

    [HttpGet("candidacies/{candidacyId:int}/types/{documentType}/versions")]
    public async Task<ActionResult<IEnumerable<DocumentVersionDto>>> GetVersionHistory(
        int candidacyId, string documentType, CancellationToken cancellationToken)
    {
        var result = await _service.GetVersionHistoryAsync(candidacyId, documentType, cancellationToken);
        return Ok(result);
    }

    // --- Required Document Definitions ---

    [HttpGet("required")]
    public async Task<ActionResult<IEnumerable<RequiredDocumentDto>>> GetRequiredDocuments(
        [FromQuery] int? callForCandidatesId,
        [FromQuery] int? orgUnitId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetRequiredDocumentsAsync(callForCandidatesId, orgUnitId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("required")]
    public async Task<ActionResult<RequiredDocumentDto>> CreateRequiredDocument(
        [FromBody] CreateRequiredDocumentCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateRequiredDocumentAsync(command, cancellationToken);
        return Created($"api/documents/required/{result.Id}", result);
    }

    [HttpDelete("required/{id:int}")]
    public async Task<IActionResult> DeleteRequiredDocument(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteRequiredDocumentAsync(id, cancellationToken);
        return NoContent();
    }

    // --- Merge ---

    [HttpPost("merge")]
    public async Task<ActionResult<MergeDocumentsResult>> Merge(
        [FromBody] MergeDocumentsCommand command, CancellationToken cancellationToken)
    {
        var result = await _mergeService.MergeAsync(command, cancellationToken);
        return Ok(result);
    }

    // --- Completeness Check ---

    [HttpGet("candidacies/{candidacyId:int}/completeness")]
    public async Task<ActionResult<DocumentCompletenessResult>> CheckCompleteness(
        int candidacyId, CancellationToken cancellationToken)
    {
        var result = await _service.CheckCompletenessAsync(candidacyId, cancellationToken);
        return Ok(result);
    }
}
