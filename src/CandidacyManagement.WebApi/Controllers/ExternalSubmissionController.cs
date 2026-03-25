using CandidacyManagement.Application.ExternalSubmissions;
using CandidacyManagement.Domain.Exceptions;
using CandidacyManagement.WebApi.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

/// <summary>
/// בקר API חיצוני לקליטת מועמדויות ממערכת הגשה חיצונית
/// מאובטח באמצעות API Key - כל קריאה דורשת כותרת X-Api-Key תקינה
/// </summary>
[ApiController]
[Route("api/external/submissions")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
public class ExternalSubmissionController : ControllerBase
{
    private readonly IExternalSubmissionService _service;

    public ExternalSubmissionController(IExternalSubmissionService service)
    {
        _service = service;
    }

    /// <summary>
    /// קליטת מועמדות חדשה ממערכת חיצונית
    /// מחזיר אישור קליטה עם מזהה מועמדות ייחודי בהצלחה,
    /// או הודעת שגיאה מפורטת עם רשימת שדות לא תקינים בכשלון
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubmissionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ExternalApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExternalApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Submit(
        [FromBody] ExternalSubmissionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.SubmitAsync(command, cancellationToken);
            return CreatedAtAction(null, new { id = result.CandidacyId }, result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ExternalApiErrorResponse(
                "VALIDATION_ERROR",
                "נתוני המועמדות אינם תקינים",
                ex.Errors));
        }
        catch (BusinessRuleViolationException ex)
        {
            return Conflict(new ExternalApiErrorResponse(
                "BUSINESS_RULE_VIOLATION",
                ex.Message,
                null));
        }
    }

    /// <summary>
    /// ולידציה של נתוני מועמדות לפני קליטה
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(SubmissionValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExternalApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SubmissionValidationResult>> Validate(
        [FromBody] ExternalSubmissionCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.ValidateAsync(command, cancellationToken);
        if (!result.IsValid)
        {
            return BadRequest(new ExternalApiErrorResponse(
                "VALIDATION_ERROR",
                "נתוני המועמדות אינם תקינים",
                result.Errors));
        }

        return Ok(result);
    }
}

/// <summary>
/// מבנה תגובת שגיאה מפורטת ל-API חיצוני
/// </summary>
public record ExternalApiErrorResponse(
    string ErrorCode,
    string Message,
    IDictionary<string, string[]>? FieldErrors);
