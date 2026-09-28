using Microsoft.EntityFrameworkCore;

namespace PluriOne.OnboardingService.Data;

public sealed class OnboardingDbContext : DbContext
{
    public OnboardingDbContext(DbContextOptions<OnboardingDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PlanPhase> PlanPhases => Set<PlanPhase>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Task> Tasks => Set<Task>();
    public DbSet<EmployeeTask> EmployeeTasks => Set<EmployeeTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("onboarding");

        modelBuilder.Entity<Employee>().ToTable("employees");
        modelBuilder.Entity<Employee>().HasIndex(e => e.UserId).IsUnique();
        modelBuilder.Entity<Employee>()
            .HasMany(e => e.Tasks)
            .WithOne()
            .HasForeignKey(et => et.EmployeeId);

        modelBuilder.Entity<PlanPhase>().ToTable("plan_phases");
        modelBuilder.Entity<PlanPhase>().HasIndex(p => p.PhaseKey).IsUnique();
        modelBuilder.Entity<PlanPhase>()
            .HasMany(p => p.Activities)
            .WithOne()
            .HasForeignKey(a => a.PhaseId);

        modelBuilder.Entity<Activity>().ToTable("activities");

        modelBuilder.Entity<Task>().ToTable("tasks");

        modelBuilder.Entity<EmployeeTask>().ToTable("employee_tasks");
        modelBuilder.Entity<EmployeeTask>().HasIndex(et => et.EmployeeId);
        modelBuilder.Entity<EmployeeTask>()
            .HasOne(et => et.Task)
            .WithMany()
            .HasForeignKey(et => et.TaskId);
    }
}
