using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CandidacyManagement.WebApi.Authentication;

/// <summary>
/// מנגנון אימות API Key למערכות חיצוניות
/// מאמת את הכותרת X-Api-Key מול רשימת מפתחות מוגדרת
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // בדיקה שהכותרת X-Api-Key קיימת
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("חסרה כותרת X-Api-Key"));
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("ערך X-Api-Key ריק"));
        }

        // חיפוש המפתח ברשימת המפתחות המוגדרים
        var apiKeyConfig = Options.ApiKeys
            .FirstOrDefault(k => string.Equals(k.Key, providedApiKey, StringComparison.Ordinal));

        if (apiKeyConfig == null)
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key לא תקין"));
        }

        // יצירת Claims עם מזהה המערכת החיצונית
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, apiKeyConfig.SystemId),
            new Claim(ClaimTypes.Name, apiKeyConfig.SystemName),
            new Claim("ExternalSystemId", apiKeyConfig.SystemId)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// הגדרות אימות API Key
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// רשימת מפתחות API מורשים
    /// </summary>
    public List<ApiKeyEntry> ApiKeys { get; set; } = new();
}

/// <summary>
/// רשומת מפתח API - מקשרת מפתח למזהה מערכת חיצונית
/// </summary>
public class ApiKeyEntry
{
    /// <summary>
    /// ערך המפתח
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// מזהה המערכת החיצונית
    /// </summary>
    public string SystemId { get; set; } = string.Empty;

    /// <summary>
    /// שם המערכת החיצונית
    /// </summary>
    public string SystemName { get; set; } = string.Empty;
}

/// <summary>
/// קבועים לסכמת אימות API Key
/// </summary>
public static class ApiKeyAuthenticationDefaults
{
    public const string AuthenticationScheme = "ApiKey";
}
