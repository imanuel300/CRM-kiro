using System.Net;
using System.Text.Json;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.WebApi.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred");
            await WriteErrorResponseAsync(context, (int)HttpStatusCode.BadRequest, new
            {
                message = ex.Message,
                errors = ex.Errors
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await WriteErrorResponseAsync(context, (int)HttpStatusCode.NotFound, new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            await WriteErrorResponseAsync(context, (int)HttpStatusCode.Forbidden, new
            {
                message = ex.Message
            });
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation");
            await WriteErrorResponseAsync(context, (int)HttpStatusCode.Conflict, new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await WriteErrorResponseAsync(context, (int)HttpStatusCode.InternalServerError, new
            {
                message = "An internal server error occurred.",
                traceId = context.TraceIdentifier
            });
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, int statusCode, object response)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
