using System.IdentityModel.Tokens.Jwt;
using PluriOne.Web.Services;

namespace PluriOne.Web.Middleware;

/// <summary>
/// Renueva automáticamente el access token usando el refresh token (cookie)
/// cuando el access token está ausente o próximo a expirar.
/// </summary>
public sealed class RefreshMiddleware
{
    private readonly RequestDelegate _next;

    public RefreshMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AuthFlowService authFlow)
    {
        var access = context.Request.Cookies["access_token"];
        var refresh = context.Request.Cookies["refresh_token"];

        if (!string.IsNullOrEmpty(refresh) && (string.IsNullOrEmpty(access) || IsExpiring(access)))
        {
            await authFlow.TryRefreshAsync();
        }

        await _next(context);
    }

    private static bool IsExpiring(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo <= DateTime.UtcNow.AddSeconds(60);
        }
        catch
        {
            return true;
        }
    }
}
