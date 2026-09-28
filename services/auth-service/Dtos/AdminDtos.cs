namespace PluriOne.AuthService.Dtos;

public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string? EmployeeCode,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record UpdateRoleRequest(string Role);

public sealed record UpdateStatusRequest(bool IsActive);

public sealed record AuditEventDto(
    Guid Id,
    Guid? UserId,
    string UserName,
    string Action,
    string? Resource,
    string? IpAddress,
    DateTimeOffset CreatedAt);

public sealed record AuditDaySummary(string Date, int Count);

public sealed record ResetPasswordRequest(string NewPassword);

public sealed record CreateUserRequest(
    string Email,
    string FullName,
    string Role,
    string Password,
    string? EmployeeCode);
