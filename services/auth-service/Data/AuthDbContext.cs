using Microsoft.EntityFrameworkCore;

namespace PluriOne.AuthService.Data;

public sealed class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        modelBuilder.Entity<Role>().ToTable("roles");
        modelBuilder.Entity<Role>().HasIndex(r => r.Name).IsUnique();

        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Property(u => u.EmployeeCode).IsRequired(false);

        modelBuilder.Entity<Session>().ToTable("sessions");
        modelBuilder.Entity<Session>().HasIndex(s => s.UserId);

        modelBuilder.Entity<LoginAttempt>().ToTable("login_attempts");

        modelBuilder.Entity<AuditEvent>().ToTable("audit_events");
    }
}
