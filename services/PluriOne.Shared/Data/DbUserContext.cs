using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PluriOne.Shared.Data;

/// <summary>
/// Ambient (AsyncLocal) context used by <see cref="PostgresUserContextInterceptor"/>
/// to set PostgreSQL session variables (app.user_id / app.role) on the pooled connection
/// for Row Level Security. Values are set per-request and cleared when the request ends.
/// </summary>
public static class DbUserContext
{
    private static readonly AsyncLocal<string?> UserIdHolder = new();
    private static readonly AsyncLocal<string?> RoleHolder = new();

    public static string? UserId => UserIdHolder.Value;
    public static string? Role => RoleHolder.Value;

    public static void Set(string? userId, string? role)
    {
        UserIdHolder.Value = userId;
        RoleHolder.Value = role;
    }

    public static void Clear()
    {
        UserIdHolder.Value = null;
        RoleHolder.Value = null;
    }
}

public sealed class DbUserContextMiddleware
{
    private readonly RequestDelegate _next;

    public DbUserContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("role");
            DbUserContext.Set(userId, role);
        }

        try
        {
            await _next(context);
        }
        finally
        {
            DbUserContext.Clear();
        }
    }
}
