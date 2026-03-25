using System.Diagnostics;
using System.Text;
using CandidacyManagement.Application.ExternalSubmissions;
using CandidacyManagement.Domain.Entities;

namespace CandidacyManagement.WebApi.Middleware;

/// <summary>
/// Middleware לתיעוד כל קריאה ל-API החיצוני
/// מתעד: תאריך, מזהה מערכת, בקשה, תגובה, תוצאה ופרטי שגיאה
/// פועל רק על נתיבי /api/external/
/// </summary>
public class ApiCallLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiCallLoggingMiddleware> _logger;
    private const string ExternalApiPrefix = "/api/external";
    private const int MaxBodyLength = 4096;

    public ApiCallLoggingMiddleware(RequestDelegate next, ILogger<ApiCallLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // תיעוד רק עבור קריאות API חיצוני
        if (!context.Request.Path.StartsWithSegments(ExternalApiPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        // קריאת גוף הבקשה
        context.Request.EnableBuffering();
        var requestBody = await ReadRequestBodyAsync(context.Request);

        // החלפת ה-Response stream כדי לתפוס את גוף התגובה
        var originalResponseBody = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        string? responseBody = null;
        string? errorDetails = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            errorDetails = ex.Message;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // קריאת גוף התגובה
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            // תיעוד הקריאה
            await LogApiCallAsync(context, requestBody, responseBody, errorDetails, stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task LogApiCallAsync(
        HttpContext context,
        string? requestBody,
        string? responseBody,
        string? errorDetails,
        long durationMs)
    {
        try
        {
            // שליפת מזהה מערכת חיצונית מה-Claims (אם אומת)
            var externalSystemId = context.User?.FindFirst("ExternalSystemId")?.Value ?? "unknown";

            var statusCode = context.Response.StatusCode;
            var isSuccess = statusCode >= 200 && statusCode < 300;

            // אם יש שגיאה בתגובה ואין errorDetails, נשלוף מגוף התגובה
            if (!isSuccess && string.IsNullOrEmpty(errorDetails) && !string.IsNullOrEmpty(responseBody))
            {
                errorDetails = TruncateBody(responseBody);
            }

            var log = new ApiCallLog
            {
                ExternalSystemId = externalSystemId,
                Endpoint = context.Request.Path.ToString(),
                HttpMethod = context.Request.Method,
                ResponseStatusCode = statusCode,
                RequestBody = TruncateBody(requestBody),
                ResponseBody = TruncateBody(responseBody),
                ErrorDetails = errorDetails,
                IsSuccess = isSuccess,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                DurationMs = durationMs,
                Timestamp = DateTime.UtcNow
            };

            // שימוש ב-scope חדש כדי לא להיתקל בבעיות Scoped services
            using var scope = context.RequestServices.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<IApiCallLogService>();
            await logService.LogAsync(log);

            _logger.LogInformation(
                "API Call: {Method} {Endpoint} by {SystemId} -> {StatusCode} ({DurationMs}ms)",
                log.HttpMethod, log.Endpoint, log.ExternalSystemId, log.ResponseStatusCode, log.DurationMs);
        }
        catch (Exception ex)
        {
            // לא נכשיל את הבקשה בגלל כשלון בתיעוד
            _logger.LogError(ex, "שגיאה בתיעוד קריאת API חיצוני");
        }
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        request.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Seek(0, SeekOrigin.Begin);
        return string.IsNullOrWhiteSpace(body) ? null : body;
    }

    private static string? TruncateBody(string? body)
    {
        if (string.IsNullOrEmpty(body))
            return null;

        return body.Length > MaxBodyLength
            ? body[..MaxBodyLength] + "...[truncated]"
            : body;
    }
}
