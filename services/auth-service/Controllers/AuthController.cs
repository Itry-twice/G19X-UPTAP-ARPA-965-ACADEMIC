using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PluriOne.AuthService.Dtos;
using PluriOne.AuthService.Services;
using PluriOne.Shared.Auth;
using PluriOne.Shared.Contracts;

namespace PluriOne.AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthManager _authManager;
    private readonly ICurrentUserContext _currentUser;

    public AuthController(AuthManager authManager, ICurrentUserContext currentUser)
    {
        _authManager = authManager;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _authManager.LoginAsync(request.Email, request.Password, ip, userAgent);

        return result.Status switch
        {
            LoginStatus.Success => Ok(result.Tokens),
            LoginStatus.InvalidCredentials => Unauthorized(new ApiError("invalid_credentials", "Credenciales inválidas.")),
            LoginStatus.LockedOut => StatusCode(StatusCodes.Status423Locked,
                new ApiError("locked_out", "Cuenta temporalmente bloqueada por intentos fallidos.")),
            _ => Unauthorized(new ApiError("login_failed", "No fue posible iniciar sesión."))
        };
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var tokens = await _authManager.RefreshAsync(request.RefreshToken, ip);

        if (tokens is null)
        {
            return Unauthorized(new ApiError("invalid_refresh_token", "Sesión inválida o expirada."));
        }

        return Ok(tokens);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await _authManager.LogoutAsync(request.RefreshToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = _currentUser.User;
        if (user is null)
        {
            return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));
        }

        var me = await _authManager.GetMeAsync(user.Id);
        return me is null ? Unauthorized(new ApiError("unauthorized", "Usuario no encontrado.")) : Ok(me);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = _currentUser.User;
        if (user is null)
        {
            return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));
        }

        var (ok, error) = await _authManager.ChangePasswordAsync(user.Id, request.CurrentPassword, request.NewPassword);
        if (!ok)
        {
            return error switch
            {
                "invalid_current" => BadRequest(new ApiError("invalid_current", "La contraseña actual es incorrecta.")),
                "weak_password" => BadRequest(new ApiError("weak_password", "La nueva contraseña debe tener al menos 8 caracteres.")),
                _ => NotFound(new ApiError("not_found", "Usuario no encontrado."))
            };
        }

        return NoContent();
    }

    [Authorize]
    [HttpPatch("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = _currentUser.User;
        if (user is null)
        {
            return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));
        }

        var updated = await _authManager.UpdateFullNameAsync(user.Id, request.FullName);
        return updated
            ? NoContent()
            : BadRequest(new ApiError("invalid_request", "Nombre inválido."));
    }
}
