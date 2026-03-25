using System.Text.Encodings.Web;
using CandidacyManagement.WebApi.Authentication;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CandidacyManagement.Application.Tests.ExternalSubmissions;

/// <summary>
/// Feature: unified-candidacy-management, Property 8: אימות חובה (Authentication Enforcement)
/// 
/// **Validates: Requirements 10.9**
/// 
/// For any API call to the external submission endpoints, if the request does not include
/// valid authentication credentials (a configured API Key in the X-Api-Key header),
/// the request is rejected with AuthenticateResult.Fail (resulting in 401 Unauthorized).
/// </summary>
public class ApiKeyAuthenticationPropertyTests
{
    /// <summary>
    /// The fixed set of valid API keys used in all test scenarios.
    /// </summary>
    private static readonly List<ApiKeyEntry> ValidApiKeys = new()
    {
        new ApiKeyEntry { Key = "valid-key-alpha-001", SystemId = "SYS-A", SystemName = "System Alpha" },
        new ApiKeyEntry { Key = "valid-key-beta-002", SystemId = "SYS-B", SystemName = "System Beta" },
        new ApiKeyEntry { Key = "valid-key-gamma-003", SystemId = "SYS-C", SystemName = "System Gamma" }
    };

    private static readonly HashSet<string> ValidKeySet =
        new(ValidApiKeys.Select(k => k.Key), StringComparer.Ordinal);

    /// <summary>
    /// Creates an ApiKeyAuthenticationHandler wired to a real HttpContext with the given header value.
    /// </summary>
    private static async Task<AuthenticateResult> AuthenticateWithHeader(string? headerValue, bool includeHeader)
    {
        var options = new ApiKeyAuthenticationOptions
        {
            ApiKeys = ValidApiKeys
        };

        var optionsMonitor = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        optionsMonitor.Setup(o => o.CurrentValue).Returns(options);
        optionsMonitor.Setup(o => o.Get(It.IsAny<string>())).Returns(options);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);

        var scheme = new AuthenticationScheme(
            ApiKeyAuthenticationDefaults.AuthenticationScheme,
            displayName: null,
            handlerType: typeof(ApiKeyAuthenticationHandler));

        var context = new DefaultHttpContext();
        if (includeHeader)
        {
            context.Request.Headers["X-Api-Key"] = headerValue;
        }

        var handler = new ApiKeyAuthenticationHandler(
            optionsMonitor.Object,
            loggerFactory.Object,
            UrlEncoder.Default);

        await handler.InitializeAsync(scheme, context);
        return await handler.AuthenticateAsync();
    }

    /// <summary>
    /// Arbitrary that generates random strings which are NOT in the valid keys set.
    /// Includes alphanumeric strings, special characters, near-miss keys, etc.
    /// </summary>
    public record InvalidKeyScenario(string ProvidedKey);

    private static Arbitrary<InvalidKeyScenario> InvalidKeyScenarioArb()
    {
        var randomStrings = Gen.Elements(
            "wrong-key", "invalid", "abc123", "VALID-KEY-ALPHA-001",
            "valid-key-alpha-001 ", " valid-key-alpha-001",
            "valid-key-alpha-001\n", "valid-key-beta",
            "valid-key-gamma-003!", "key-that-does-not-exist",
            "!@#$%^&*()", "SELECT * FROM keys", "<script>alert(1)</script>",
            "valid-key-alpha-002", "valid-key-delta-004",
            new string('a', 500), new string('x', 1),
            "null", "undefined", "true", "false", "0", "-1");

        var arbitraryStrings = Arb.Default.NonEmptyString().Generator
            .Select(s => s.Get)
            .Where(s => !ValidKeySet.Contains(s));

        var combined = Gen.OneOf(randomStrings, arbitraryStrings);

        return Arb.From(combined.Select(k => new InvalidKeyScenario(k)));
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 8: אימות חובה
    /// **Validates: Requirements 10.9**
    /// 
    /// For any randomly generated API key string that is NOT in the configured valid keys list,
    /// the ApiKeyAuthenticationHandler returns AuthenticateResult.Fail (401 Unauthorized).
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ApiKeyAuthenticationPropertyTests) })]
    public async Task<bool> InvalidApiKeyAlwaysResultsInAuthenticationFailure(InvalidKeyScenario scenario)
    {
        var result = await AuthenticateWithHeader(scenario.ProvidedKey, includeHeader: true);

        // Authentication must fail for any key not in the valid set
        return !result.Succeeded;
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 8: אימות חובה
    /// **Validates: Requirements 10.9**
    /// 
    /// When the X-Api-Key header is missing entirely, authentication fails.
    /// </summary>
    [Fact]
    public async Task MissingApiKeyHeaderResultsInAuthenticationFailure()
    {
        var result = await AuthenticateWithHeader(headerValue: null, includeHeader: false);
        Assert.False(result.Succeeded);
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 8: אימות חובה
    /// **Validates: Requirements 10.9**
    /// 
    /// When the X-Api-Key header is present but empty or whitespace, authentication fails.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task EmptyOrWhitespaceApiKeyResultsInAuthenticationFailure(string headerValue)
    {
        var result = await AuthenticateWithHeader(headerValue, includeHeader: true);
        Assert.False(result.Succeeded);
    }

    /// <summary>
    /// Sanity check: a valid key DOES succeed (ensures the test setup is correct).
    /// </summary>
    [Fact]
    public async Task ValidApiKeySucceedsAuthentication()
    {
        var result = await AuthenticateWithHeader("valid-key-alpha-001", includeHeader: true);
        Assert.True(result.Succeeded);
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<InvalidKeyScenario> Arbitrary() => InvalidKeyScenarioArb();
}
