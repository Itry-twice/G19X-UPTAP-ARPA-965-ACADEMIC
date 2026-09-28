using System.Text.Json;
using PluriOne.Web.Contracts;

namespace PluriOne.Web.Services;

public enum LoginResult
{
    Success,
    InvalidCredentials,
    LockedOut,
    Error
}

/// <summary>
/// Gestiona el flujo de autenticación del BFF: login, refresh y logout,
/// manteniendo los tokens en cookies HttpOnly.
/// </summary>
public sealed class AuthFlowService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthFlowService> _logger;

    public AuthFlowService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<AuthFlowService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var payload = new { email, password };

        try
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", payload);

            if (response.IsSuccessStatusCode)
            {
                var login = await ReadAsync<LoginResponse>(response);
                if (login is not null)
                {
                    SetAuthCookies(login.AccessToken, login.RefreshToken);
                    return LoginResult.Success;
                }
                return LoginResult.Error;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return LoginResult.InvalidCredentials;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Locked)
            {
                return LoginResult.LockedOut;
            }

            return LoginResult.Error;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesión");
            return LoginResult.Error;
        }
    }

    public async Task<bool> TryRefreshAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return false;

        var refreshToken = context.Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken)) return false;

        try
        {
            var client = _httpClientFactory.CreateClient("auth");
            var response = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var tokens = await ReadAsync<LoginResponse>(response);
            if (tokens is null)
            {
                return false;
            }

            SetAuthCookies(tokens.AccessToken, tokens.RefreshToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al renovar la sesión");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return;

        var refreshToken = context.Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            try
            {
                var client = _httpClientFactory.CreateClient("auth");
                var response = await client.PostAsJsonAsync("/api/auth/logout", new { refreshToken });
                _ = response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión en AuthService");
            }
        }

        context.Response.Cookies.Delete("access_token");
        context.Response.Cookies.Delete("refresh_token");
    }

    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return;

        var secure = bool.TryParse(_configuration["SessionCookieSecure"], out var s) && s;

        var accessOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(30)
        };
        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromDays(7)
        };

        context.Response.Cookies.Append("access_token", accessToken, accessOptions);
        context.Response.Cookies.Append("refresh_token", refreshToken, refreshOptions);
    }

    private static async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
