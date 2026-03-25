using System.Security.Claims;
using CandidacyManagement.Application.Roles;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.WebApi.Middleware;

/// <summary>
/// Middleware להגבלת גישה לפי תפקיד ויחידה ארגונית
/// בודק הרשאות משתמש לפי ה-Claims ב-JWT ולפי נתיב הבקשה
/// </summary>
public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    // מיפוי שיטות HTTP להרשאות נדרשות
    private static readonly Dictionary<string, PermissionType> MethodPermissionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GET"] = PermissionType.View,
        ["POST"] = PermissionType.Create,
        ["PUT"] = PermissionType.Edit,
        ["PATCH"] = PermissionType.Edit,
        ["DELETE"] = PermissionType.Delete
    };

    // נתיבים שלא דורשים בדיקת הרשאות
    private static readonly string[] ExcludedPaths = { "/api/auth", "/api/health", "/api/external" };

    public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // דילוג על נתיבים שלא דורשים בדיקה
        if (ExcludedPaths.Any(ep => path.StartsWith(ep, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // דילוג אם המשתמש לא מאומת
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? context.User.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            await _next(context);
            return;
        }

        // שליפת OrgUnitId מה-Query string או מה-Route
        var orgUnitId = GetOrgUnitIdFromRequest(context);
        if (!orgUnitId.HasValue)
        {
            // אם אין OrgUnitId בבקשה, ממשיכים (הבדיקה תתבצע ברמת ה-Service)
            await _next(context);
            return;
        }

        // קביעת ההרשאה הנדרשת לפי שיטת HTTP
        if (!MethodPermissionMap.TryGetValue(context.Request.Method, out var requiredPermission))
        {
            await _next(context);
            return;
        }

        // בדיקת סטטוס-ספציפי
        if (path.Contains("/status", StringComparison.OrdinalIgnoreCase) &&
            (context.Request.Method == "PUT" || context.Request.Method == "PATCH"))
        {
            requiredPermission = PermissionType.ChangeStatus;
        }

        // בדיקת דיוור
        if (path.Contains("/notifications/send", StringComparison.OrdinalIgnoreCase) &&
            context.Request.Method == "POST")
        {
            requiredPermission = PermissionType.SendNotification;
        }

        using var scope = context.RequestServices.CreateScope();
        var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();

        var hasPermission = await roleService.HasPermissionAsync(
            userId, orgUnitId.Value, requiredPermission);

        if (!hasPermission)
        {
            _logger.LogWarning(
                "Access denied: User {UserId} attempted {Permission} on OrgUnit {OrgUnitId} at {Path}",
                userId, requiredPermission, orgUnitId.Value, path);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "אין לך הרשאה לבצע פעולה זו" });
            return;
        }

        // תיעוד הפעולה ב-Audit Log
        await roleService.LogActionAsync(new CreateAuditLogCommand(
            UserId: userId,
            Action: $"{context.Request.Method} {path}",
            EntityType: ExtractEntityTypeFromPath(path),
            EntityId: ExtractEntityIdFromPath(path),
            OrgUnitId: orgUnitId,
            Details: null));

        await _next(context);
    }

    private static int? GetOrgUnitIdFromRequest(HttpContext context)
    {
        // מ-Query string
        if (context.Request.Query.TryGetValue("orgUnitId", out var queryValue) &&
            int.TryParse(queryValue.FirstOrDefault(), out var fromQuery))
            return fromQuery;

        // מ-Route values
        if (context.Request.RouteValues.TryGetValue("orgUnitId", out var routeValue) &&
            routeValue is string routeStr && int.TryParse(routeStr, out var fromRoute))
            return fromRoute;

        // מ-Header
        if (context.Request.Headers.TryGetValue("X-OrgUnit-Id", out var headerValue) &&
            int.TryParse(headerValue.FirstOrDefault(), out var fromHeader))
            return fromHeader;

        return null;
    }

    private static string ExtractEntityTypeFromPath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        // /api/{entityType}/... -> entityType
        return segments.Length >= 2 ? segments[1] : "unknown";
    }

    private static int? ExtractEntityIdFromPath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        // /api/{entityType}/{id} -> id
        if (segments.Length >= 3 && int.TryParse(segments[2], out var id))
            return id;
        return null;
    }
}
