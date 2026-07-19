using GradGateway.Business.Helpers;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GradGateway.Api.Middleware;

/// <summary>
/// Enforces account suspension and maintenance mode on authenticated API traffic.
/// </summary>
public class PlatformAccessMiddleware
{
    private readonly RequestDelegate _next;

    public PlatformAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, GradGatewayDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/hubs", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var firebaseUid = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("user_id")?.Value;

        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            await _next(context);
            return;
        }

        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

        if (user == null)
        {
            await _next(context);
            return;
        }

        if (!user.IsActive)
        {
            var isStatusCheck = path.StartsWith("/api/auth/me", StringComparison.OrdinalIgnoreCase);
            var isSupportRequest =
                path.StartsWith("/api/supportinquiries", StringComparison.OrdinalIgnoreCase) &&
                context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);

            if (isStatusCheck || isSupportRequest)
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Your account has been blocked by an administrator. Contact support if you believe this is a mistake.",
                code = "ACCOUNT_BLOCKED"
            });
            return;
        }

        var settings = await PlatformSettingsAccessor.GetOrCreateAsync(db);
        if (settings.MaintenanceMode && user.Role != UserRole.Admin)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "The platform is under maintenance. Please try again later."
            });
            return;
        }

        await _next(context);
    }
}
