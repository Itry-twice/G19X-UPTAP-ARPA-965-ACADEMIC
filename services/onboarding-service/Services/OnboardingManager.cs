using Microsoft.EntityFrameworkCore;
using PluriOne.OnboardingService.Data;
using PluriOne.OnboardingService.Dtos;
using PluriOne.Shared.Auth;

namespace PluriOne.OnboardingService.Services;

public sealed class OnboardingManager
{
    private static readonly string[] AllowedStatuses = { "pending", "in_progress", "completed" };

    private readonly OnboardingDbContext _db;

    public OnboardingManager(OnboardingDbContext db) => _db = db;

    public async Task<Employee> GetOrCreateEmployeeAsync(CurrentUser user)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
        if (employee is not null)
        {
            return employee;
        }

        employee = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName,
            Email = user.Email,
            Position = "Nuevo colaborador (DEMO)",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Employees.Add(employee);

        var taskIds = await _db.Tasks.OrderBy(t => t.SortOrder).Select(t => t.Id).ToListAsync();
        foreach (var taskId in taskIds)
        {
            _db.EmployeeTasks.Add(new EmployeeTask
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                TaskId = taskId,
                Status = "pending"
            });
        }

        await _db.SaveChangesAsync();
        return employee;
    }

    public async Task<List<TaskItemDto>> GetTasksAsync(Guid employeeId)
    {
        var employeeTasks = await _db.EmployeeTasks
            .Where(et => et.EmployeeId == employeeId)
            .Include(et => et.Task)
            .ToListAsync();

        return employeeTasks
            .OrderBy(et => et.Task?.SortOrder)
            .Select(et => new TaskItemDto(
                et.TaskId,
                et.Task?.Title ?? string.Empty,
                et.Task?.Description,
                et.Task?.Category,
                et.Task?.IsRequired ?? false,
                et.Status,
                et.CompletedAt))
            .ToList();
    }

    public async Task<bool> UpdateTaskStatusAsync(Guid employeeId, Guid taskId, string status)
    {
        if (!AllowedStatuses.Contains(status))
        {
            return false;
        }

        var employeeTask = await _db.EmployeeTasks
            .FirstOrDefaultAsync(et => et.EmployeeId == employeeId && et.TaskId == taskId);

        if (employeeTask is null)
        {
            return false;
        }

        employeeTask.Status = status;
        employeeTask.CompletedAt = status == "completed" ? DateTimeOffset.UtcNow : null;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ProgressDto> GetProgressAsync(Guid employeeId)
    {
        var items = await _db.EmployeeTasks
            .Where(et => et.EmployeeId == employeeId)
            .Select(et => et.Status)
            .ToListAsync();

        var total = items.Count;
        var completed = items.Count(s => s == "completed");
        var inProgress = items.Count(s => s == "in_progress");
        var pending = items.Count(s => s == "pending");
        var percentage = total == 0 ? 0 : (int)Math.Round(completed * 100.0 / total);

        return new ProgressDto(total, completed, inProgress, pending, percentage);
    }

    public async Task<List<PlanPhaseDto>> GetPlanAsync()
    {
        var phases = await _db.PlanPhases
            .OrderBy(p => p.SortOrder)
            .Include(p => p.Activities)
            .ToListAsync();

        return phases.Select(p => new PlanPhaseDto(
            p.Id,
            p.PhaseKey,
            p.Name,
            p.DaysStart,
            p.DaysEnd,
            p.Objective,
            (p.Activities ?? new List<Activity>())
                .OrderBy(a => a.SortOrder)
                .Select(a => new ActivityDto(a.Id, a.Title, a.Description))
                .ToList()))
            .ToList();
    }
}
