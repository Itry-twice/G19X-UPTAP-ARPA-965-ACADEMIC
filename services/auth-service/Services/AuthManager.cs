using Microsoft.EntityFrameworkCore;
using PluriOne.AuthService.Data;
using PluriOne.AuthService.Dtos;
using PluriOne.Shared.Auth;

namespace PluriOne.AuthService.Services;

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    LockedOut
}

public sealed record LoginResult(LoginStatus Status, TokenResponse? Tokens);

public sealed class AuthManager
{
    private readonly AuthDbContext _db;
    private readonly PasswordHasher _hasher;
    private readonly JwtTokenService _jwt;
    private readonly LoginRateLimiter _limiter;
    private readonly JwtOptions _options;

    public AuthManager(
        AuthDbContext db,
        PasswordHasher hasher,
        JwtTokenService jwt,
        LoginRateLimiter limiter,
        Microsoft.Extensions.Options.IOptions<JwtOptions> options)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _limiter = limiter;
        _options = options.Value;
    }

    public async Task<LoginResult> LoginAsync(string email, string password, string? ip, string? userAgent)
    {
        email = email.Trim().ToLowerInvariant();

        if (await _limiter.IsLockedAsync(email, ip ?? "unknown"))
        {
            return new LoginResult(LoginStatus.LockedOut, null);
        }

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user is null || !_hasher.Verify(password, user.PasswordHash))
        {
            await _limiter.RegisterFailureAsync(email, ip ?? "unknown");
            await RecordAttempt(email, ip, success: false);
            await Audit(null, null, "login.failed", "user", $"login failed for {email}");
            return new LoginResult(LoginStatus.InvalidCredentials, null);
        }

        await _limiter.ResetAsync(email, ip ?? "unknown");
        await RecordAttempt(email, ip, success: true);

        var tokens = await IssueTokensAsync(user, user.Role?.Name ?? "colaborador", ip, userAgent);
        await Audit(user.Id, user.FullName, "login.success", "user", $"login succeeded for {email}");
        return new LoginResult(LoginStatus.Success, tokens);
    }

    public async Task<TokenResponse?> RefreshAsync(string refreshToken, string? ip)
    {
        var tokenHash = PasswordHasher.HashToken(refreshToken);
        var session = await _db.Sessions
            .Include(s => s.User!)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == tokenHash);

        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        var user = session.User;
        if (user is null || !user.IsActive)
        {
            return null;
        }

        // Rotación: revocar la sesión actual y emitir una nueva.
        session.RevokedAt = DateTimeOffset.UtcNow;
        var tokens = await IssueTokensAsync(user, user.Role?.Name ?? "colaborador", ip, session.UserAgent);
        await Audit(session.UserId, user.FullName, "token.refresh", "session", null);
        return tokens;
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var tokenHash = PasswordHasher.HashToken(refreshToken);
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.RefreshTokenHash == tokenHash);
        if (session is null || session.RevokedAt is not null)
        {
            return false;
        }

        session.RevokedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        await Audit(session.UserId, session.User?.FullName, "logout", "session", null);
        return true;
    }

    public async Task<MeResponse?> GetMeAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user is null)
        {
            return null;
        }

        return new MeResponse(user.Id, user.Email, user.FullName, user.Role?.Name ?? "colaborador", user.EmployeeCode);
    }

    public async Task<(bool Ok, string? Error)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return (false, "not_found");
        }

        if (!_hasher.Verify(currentPassword, user.PasswordHash))
        {
            return (false, "invalid_current");
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            return (false, "weak_password");
        }

        user.PasswordHash = _hasher.Hash(newPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        await Audit(user.Id, user.FullName, "password.changed", "user", null);
        return (true, null);
    }

    public async Task<bool> UpdateFullNameAsync(Guid userId, string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return false;
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return false;
        }

        user.FullName = fullName.Trim();
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<TokenResponse> IssueTokensAsync(User user, string role, string? ip, string? userAgent)
    {
        var currentUser = new CurrentUser(user.Id, user.Email, user.FullName, role);
        var accessToken = _jwt.CreateAccessToken(currentUser);
        var refreshToken = PasswordHasher.GenerateRefreshToken();

        _db.Sessions.Add(new Session
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshTokenHash = PasswordHasher.HashToken(refreshToken),
            IpAddress = ip,
            UserAgent = Truncate(userAgent, 256),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_options.RefreshDays)
        });

        await _db.SaveChangesAsync();

        return new TokenResponse(
            accessToken,
            refreshToken,
            _options.AccessMinutes * 60,
            new UserDto(user.Id, user.Email, user.FullName, role, user.EmployeeCode));
    }

    private async Task RecordAttempt(string email, string? ip, bool success)
    {
        _db.LoginAttempts.Add(new LoginAttempt
        {
            Id = Guid.NewGuid(),
            Email = email,
            IpAddress = ip,
            Success = success,
            AttemptedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    private async Task Audit(Guid? userId, string? userName, string action, string resource, string? metadata)
    {
        _db.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserName = userName ?? string.Empty,
            Action = action,
            Resource = resource,
            Metadata = metadata,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    private static string? Truncate(string? value, int max)
    {
        if (value is null || value.Length <= max) return value;
        return value[..max];
    }
}
