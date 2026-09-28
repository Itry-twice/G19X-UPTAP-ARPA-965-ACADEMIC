using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace PluriOne.Shared.Auth;

public interface ICurrentUserContext
{
    CurrentUser? User { get; }
}

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser? User
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null || principal.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var name = principal.FindFirstValue("name") ?? principal.FindFirstValue(ClaimTypes.Name);
            var role = principal.FindFirstValue(ClaimTypes.Role) ?? principal.FindFirstValue(JwtTokenService.RoleClaim);

            if (!Guid.TryParse(id, out var userId) || string.IsNullOrEmpty(email))
            {
                return null;
            }

            return new CurrentUser(userId, email, name ?? string.Empty, role ?? "colaborador");
        }
    }
}
