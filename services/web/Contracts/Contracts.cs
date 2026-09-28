namespace PluriOne.Web.Contracts;

// Auth
public sealed record LoginResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds, UserDto User);
public sealed record UserDto(Guid Id, string Email, string FullName, string Role, string? EmployeeCode);

// Onboarding
public sealed record EmployeeDto(Guid Id, string FullName, string Email, string? Position, DateOnly? StartDate);
public sealed record TaskItemDto(Guid TaskId, string Title, string? Description, string? Category, bool IsRequired, string Status, DateTimeOffset? CompletedAt);
public sealed record ProgressDto(int Total, int Completed, int InProgress, int Pending, int Percentage);
public sealed record ActivityDto(Guid Id, string Title, string? Description);
public sealed record PlanPhaseDto(Guid Id, string PhaseKey, string Name, int DaysStart, int DaysEnd, string? Objective, List<ActivityDto> Activities);

// HR
public sealed record CategoryDto(Guid Id, string Name, string? Description);
public sealed record CommentDto(Guid Id, Guid AuthorUserId, string AuthorName, string Body, DateTimeOffset CreatedAt);
public sealed record StatusHistoryDto(string? FromStatus, string ToStatus, DateTimeOffset ChangedAt);
public sealed record TicketDto(Guid Id, string Subject, string? Description, Guid? CategoryId, string? CategoryName, Guid RequesterUserId, string RequesterName, Guid? AssigneeUserId, string Status, string Priority, string Source, bool Escalated, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
public sealed record TicketDetailDto(Guid Id, string Subject, string? Description, Guid? CategoryId, string? CategoryName, Guid RequesterUserId, string RequesterName, Guid? AssigneeUserId, string Status, string Priority, string Source, bool Escalated, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, List<CommentDto> Comments, List<StatusHistoryDto> History);

// Admin
public sealed record AdminUserDto(Guid Id, string Email, string FullName, string Role, string? EmployeeCode, bool IsActive, DateTimeOffset CreatedAt);
public sealed record AuditEventDto(Guid Id, Guid? UserId, string UserName, string Action, string? Resource, string? IpAddress, DateTimeOffset CreatedAt);
public sealed record AuditDaySummary(string Date, int Count);
public sealed record DocumentDto(string Id, string Name, int Chunks, string IngestedAt);
