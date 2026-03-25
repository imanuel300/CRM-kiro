using CandidacyManagement.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace CandidacyManagement.WebApi.Controllers;

[ApiController]
[Route("api/security")]
public class SecurityController : ControllerBase
{
    private readonly ISecurityService _securityService;

    public SecurityController(ISecurityService securityService)
    {
        _securityService = securityService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(
        [FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _securityService.LoginAsync(command, cancellationToken);
        if (!result.Success && !result.RequiresMfa)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("mfa/verify")]
    public async Task<ActionResult<MfaVerificationResult>> VerifyMfa(
        [FromBody] VerifyMfaCommand command, CancellationToken cancellationToken)
    {
        var result = await _securityService.VerifyMfaAsync(command, cancellationToken);
        if (!result.Success)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpGet("session/{userId:int}")]
    public async Task<ActionResult<SessionCheckResult>> CheckSession(
        int userId, CancellationToken cancellationToken)
    {
        var result = await _securityService.CheckSessionAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("session/{userId:int}/refresh")]
    public async Task<IActionResult> RefreshSession(
        int userId, CancellationToken cancellationToken)
    {
        await _securityService.RefreshSessionAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("personal-data/erase")]
    public async Task<IActionResult> ErasePersonalData(
        [FromBody] ErasePersonalDataCommand command, CancellationToken cancellationToken)
    {
        await _securityService.ErasePersonalDataAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("accounts/{id:int}/unlock")]
    public async Task<IActionResult> UnlockAccount(
        int id, [FromBody] UnlockAccountCommand command, CancellationToken cancellationToken)
    {
        if (id != command.UserAccountId)
            return BadRequest(new { message = "Route id does not match command UserAccountId" });

        await _securityService.UnlockAccountAsync(command, cancellationToken);
        return NoContent();
    }
}
