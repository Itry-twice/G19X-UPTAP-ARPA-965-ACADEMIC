namespace PluriOne.OnboardingService.Data;

public sealed class Employee
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Position { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<EmployeeTask>? Tasks { get; set; }
}

public sealed class PlanPhase
{
    public Guid Id { get; set; }
    public string PhaseKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DaysStart { get; set; }
    public int DaysEnd { get; set; }
    public string? Objective { get; set; }
    public int SortOrder { get; set; }
    public List<Activity>? Activities { get; set; }
}

public sealed class Activity
{
    public Guid Id { get; set; }
    public Guid PhaseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public sealed class Task
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}

public sealed class EmployeeTask
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid TaskId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTimeOffset? CompletedAt { get; set; }
    public Task? Task { get; set; }
}
