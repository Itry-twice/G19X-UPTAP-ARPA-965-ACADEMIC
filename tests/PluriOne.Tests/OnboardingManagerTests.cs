using Microsoft.EntityFrameworkCore;
using PluriOne.OnboardingService.Services;
using PluriOne.Shared.Auth;
using Xunit;
using OnboardingDbContext = PluriOne.OnboardingService.Data.OnboardingDbContext;
using OnbTask = PluriOne.OnboardingService.Data.Task;

namespace PluriOne.Tests;

public sealed class OnboardingManagerTests
{
    private static OnboardingDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<OnboardingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new OnboardingDbContext(options);
        db.Tasks.Add(new OnbTask { Id = Guid.NewGuid(), Title = "Configurar VPN", SortOrder = 1 });
        db.Tasks.Add(new OnbTask { Id = Guid.NewGuid(), Title = "Leer manual", SortOrder = 2 });
        db.SaveChanges();
        return db;
    }

    private static CurrentUser User() =>
        new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "c@example.com", "C", "colaborador");

    [Fact]
    public async Task GetOrCreateEmployee_AssignsAllTasks()
    {
        await using var db = CreateDb();
        var manager = new OnboardingManager(db);

        var employee = await manager.GetOrCreateEmployeeAsync(User());

        var taskCount = await db.EmployeeTasks.CountAsync(et => et.EmployeeId == employee.Id);
        Assert.Equal(2, taskCount);
    }

    [Fact]
    public async Task GetOrCreateEmployee_IsIdempotent()
    {
        await using var db = CreateDb();
        var manager = new OnboardingManager(db);

        var e1 = await manager.GetOrCreateEmployeeAsync(User());
        var e2 = await manager.GetOrCreateEmployeeAsync(User());

        Assert.Equal(e1.Id, e2.Id);
        var employeeCount = await db.Employees.CountAsync();
        Assert.Equal(1, employeeCount);
    }

    [Fact]
    public async Task UpdateTaskStatus_RejectsInvalidStatus()
    {
        await using var db = CreateDb();
        var manager = new OnboardingManager(db);

        var employee = await manager.GetOrCreateEmployeeAsync(User());
        var task = await db.Tasks.FirstAsync();

        var ok = await manager.UpdateTaskStatusAsync(employee.Id, task.Id, "bogus");

        Assert.False(ok);
    }

    [Fact]
    public async Task Progress_ReflectsCompletedTasks()
    {
        await using var db = CreateDb();
        var manager = new OnboardingManager(db);

        var employee = await manager.GetOrCreateEmployeeAsync(User());
        var tasks = await db.Tasks.OrderBy(t => t.SortOrder).ToListAsync();
        await manager.UpdateTaskStatusAsync(employee.Id, tasks[0].Id, "completed");

        var progress = await manager.GetProgressAsync(employee.Id);

        Assert.Equal(2, progress.Total);
        Assert.Equal(1, progress.Completed);
        Assert.Equal(50, progress.Percentage);
    }
}
