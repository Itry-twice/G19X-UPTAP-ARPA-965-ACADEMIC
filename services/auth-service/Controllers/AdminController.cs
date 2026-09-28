using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PluriOne.AuthService.Dtos;
using PluriOne.AuthService.Services;
using PluriOne.Shared.Auth;
using PluriOne.Shared.Contracts;

namespace PluriOne.AuthService.Controllers;

[ApiController]
[Authorize]
[Route("api/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly AdminManager _adminManager;
    private readonly ICurrentUserContext _currentUser;

    public AdminController(AdminManager adminManager, ICurrentUserContext currentUser)
    {
        _adminManager = adminManager;
        _currentUser = currentUser;
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        if (!RequireAdmin(out var admin))
        {
            return ForbidResponse();
        }

        return Ok(await _adminManager.ListUsersAsync());
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!RequireAdmin(out var admin))
        {
            return ForbidResponse();
        }

        var (user, error) = await _adminManager.CreateUserAsync(request);
        if (user is null)
        {
            return error switch
            {
                "email_exists" => BadRequest(new ApiError("email_exists", "Ya existe un usuario con ese correo.")),
                "weak_password" => BadRequest(new ApiError("weak_password", "La contraseña debe tener al menos 8 caracteres.")),
                "invalid_role" => BadRequest(new ApiError("invalid_role", "Rol inválido.")),
                _ => BadRequest(new ApiError("invalid_request", "Datos inválidos."))
            };
        }

        return CreatedAtAction(nameof(Users), new { id = user.Id }, user);
    }

    [HttpPatch("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        if (!RequireAdmin(out var admin))
        {
            return ForbidResponse();
        }

        var (ok, error) = await _adminManager.UpdateRoleAsync(id, request.Role, admin!.Id);
        if (!ok)
        {
            return error switch
            {
                "not_found" => NotFound(new ApiError("not_found", "Usuario no encontrado.")),
                "cannot_change_own_role" => BadRequest(new ApiError("cannot_change_own_role", "No puedes cambiar tu propio rol.")),
                _ => BadRequest(new ApiError("invalid_role", "Rol inválido."))
            };
        }

        return NoContent();
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        if (!RequireAdmin(out var admin))
        {
            return ForbidResponse();
        }

        var (ok, error) = await _adminManager.UpdateStatusAsync(id, request.IsActive, admin!.Id);
        if (!ok)
        {
            return error switch
            {
                "not_found" => NotFound(new ApiError("not_found", "Usuario no encontrado.")),
                _ => BadRequest(new ApiError("cannot_change_own_status", "No puedes cambiar tu propio estado."))
            };
        }

        return NoContent();
    }

    [HttpGet("audit")]
    public async Task<IActionResult> Audit()
    {
        if (!RequireAdmin(out _))
        {
            return ForbidResponse();
        }

        return Ok(await _adminManager.ListAuditAsync());
    }

    [HttpGet("audit/by-date")]
    public async Task<IActionResult> AuditByDate([FromQuery] string date)
    {
        if (!RequireAdmin(out _))
        {
            return ForbidResponse();
        }

        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var parsed))
        {
            return BadRequest(new ApiError("invalid_date", "Fecha inválida. Use yyyy-MM-dd."));
        }

        var day = parsed.ToDateTime(TimeOnly.MinValue);
        return Ok(await _adminManager.ListAuditByDateAsync(day));
    }

    [HttpGet("audit/summary")]
    public async Task<IActionResult> AuditSummary([FromQuery] int year, [FromQuery] int month)
    {
        if (!RequireAdmin(out _))
        {
            return ForbidResponse();
        }

        if (year < 2000 || year > 2100 || month < 1 || month > 12)
        {
            return BadRequest(new ApiError("invalid_range", "Año o mes inválido."));
        }

        return Ok(await _adminManager.ListAuditSummaryAsync(year, month));
    }

    [HttpPost("users/{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request)
    {
        if (!RequireAdmin(out _))
        {
            return ForbidResponse();
        }

        var (ok, error) = await _adminManager.ResetPasswordAsync(id, request.NewPassword);
        if (!ok)
        {
            return error switch
            {
                "not_found" => NotFound(new ApiError("not_found", "Usuario no encontrado.")),
                _ => BadRequest(new ApiError("weak_password", "La contraseña debe tener al menos 8 caracteres."))
            };
        }

        return NoContent();
    }

    private bool RequireAdmin(out CurrentUser? admin)
    {
        admin = _currentUser.User;
        return admin?.IsAdmin == true;
    }

    private IActionResult ForbidResponse()
    {
        return StatusCode(StatusCodes.Status403Forbidden,
            new ApiError("forbidden", "Se requiere rol de administrador."));
    }
}
