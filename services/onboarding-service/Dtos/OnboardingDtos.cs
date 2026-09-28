namespace PluriOne.OnboardingService.Dtos;

public sealed record EmployeeDto(
    Guid Id,
    string FullName,
    string Email,
    string? Position,
    DateOnly? StartDate);

public sealed record TaskItemDto(
    Guid TaskId,
    string Title,
    string? Description,
    string? Category,
    bool IsRequired,
    string Status,
    DateTimeOffset? CompletedAt);

public sealed record ProgressDto(
    int Total,
    int Completed,
    int InProgress,
    int Pending,
    int Percentage);

public sealed record ActivityDto(Guid Id, string Title, string? Description);

public sealed record PlanPhaseDto(
    Guid Id,
    string PhaseKey,
    string Name,
    int DaysStart,
    int DaysEnd,
    string? Objective,
    List<ActivityDto> Activities);

public sealed record UpdateTaskStatusRequest(string Status);
