using Microsoft.EntityFrameworkCore;
using PluriOne.AuthService.Data;
using PluriOne.AuthService.Dtos;

namespace PluriOne.AuthService.Services;

public sealed class AdminManager
{
    private static readonly string[] AllowedRoles = { "colaborador", "rrhh", "admin" };

    private readonly AuthDbContext _db;
    private readonly PasswordHasher _hasher;

    public AdminManager(AuthDbContext db, PasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<(AdminUserDto? User, string? Error)> CreateUserAsync(CreateUserRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.FullName))
        {
            return (null, "invalid_request");
        }

        if (!AllowedRoles.Contains(request.Role))
        {
            return (null, "invalid_role");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return (null, "weak_password");
        }

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return (null, "email_exists");
        }

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
        if (role is null)
        {
            return (null, "invalid_role");
        }

        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            RoleId = role.Id,
            EmployeeCode = string.IsNullOrWhiteSpace(request.EmployeeCode) ? null : request.EmployeeCode.Trim(),
            IsActive = true,
            MustChangePassword = false,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return (new AdminUserDto(user.Id, user.Email, user.FullName, request.Role, user.EmployeeCode, true, now), null);
    }

    public async Task<List<AdminUserDto>> ListUsersAsync()
    {
        return await _db.Users
            .Include(u => u.Role)
            .OrderBy(u => u.FullName)
            .Select(u => new AdminUserDto(
                u.Id, u.Email, u.FullName, u.Role!.Name, u.EmployeeCode, u.IsActive, u.CreatedAt))
            .ToListAsync();
    }

    public async Task<(bool Ok, string? Error)> UpdateRoleAsync(Guid userId, string roleName, Guid currentAdminId)
    {
        if (!AllowedRoles.Contains(roleName))
        {
            return (false, "invalid_role");
        }

        if (userId == currentAdminId)
        {
            return (false, "cannot_change_own_role");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return (false, "not_found");
        }

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role is null)
        {
            return (false, "invalid_role");
        }

        user.RoleId = role.Id;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> UpdateStatusAsync(Guid userId, bool isActive, Guid currentAdminId)
    {
        if (userId == currentAdminId)
        {
            return (false, "cannot_change_own_status");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return (false, "not_found");
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<AuditEventDto>> ListAuditAsync(int limit = 100)
    {
        return await _db.AuditEvents
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Select(a => new AuditEventDto(a.Id, a.UserId, a.UserName, a.Action, a.Resource, a.IpAddress, a.CreatedAt))
            .ToListAsync();
    }

    public async Task<List<AuditEventDto>> ListAuditByDateAsync(DateTime date)
    {
        var start = date.ToUniversalTime();
        var end = start.AddDays(1);

        return await _db.AuditEvents
            .Where(a => a.CreatedAt >= start && a.CreatedAt < end)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AuditEventDto(a.Id, a.UserId, a.UserName, a.Action, a.Resource, a.IpAddress, a.CreatedAt))
            .ToListAsync();
    }

    public async Task<List<AuditDaySummary>> ListAuditSummaryAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);

        var events = await _db.AuditEvents
            .Where(a => a.CreatedAt >= start && a.CreatedAt < end)
            .Select(a => a.CreatedAt)
            .ToListAsync();

        return events
            .GroupBy(e => DateOnly.FromDateTime(e.UtcDateTime))
            .Select(g => new AuditDaySummary(g.Key.ToString("yyyy-MM-dd"), g.Count()))
            .ToList();
    }

    public async Task<(bool Ok, string? Error)> ResetPasswordAsync(Guid userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            return (false, "weak_password");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return (false, "not_found");
        }

        user.PasswordHash = _hasher.Hash(newPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return (true, null);
    }
}
