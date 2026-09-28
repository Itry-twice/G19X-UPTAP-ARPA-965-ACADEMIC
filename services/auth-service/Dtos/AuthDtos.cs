namespace PluriOne.AuthService.Dtos;

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record UserDto(Guid Id, string Email, string FullName, string Role, string? EmployeeCode);

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    UserDto User);

public sealed record MeResponse(Guid Id, string Email, string FullName, string Role, string? EmployeeCode);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record UpdateProfileRequest(string FullName);
