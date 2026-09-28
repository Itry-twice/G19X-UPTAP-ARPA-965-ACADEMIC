using Microsoft.EntityFrameworkCore;
using PluriOne.AuthService.Data;
using PluriOne.AuthService.Services;

namespace PluriOne.AuthService.Data;

/// <summary>
/// Siembra roles y usuarios DEMO con UUIDs fijos para permitir que otros
/// servicios referencien esos identificadores en sus datos de referencia.
/// Las credenciales DEMO se documentan en el README.
/// </summary>
public sealed class DemoSeeder
{
    public static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid RrhhId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid EmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public const string DemoPassword = "PluriOne#2026";

    private readonly AuthDbContext _db;
    private readonly PasswordHasher _hasher;

    public DemoSeeder(AuthDbContext db, PasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task SeedAsync()
    {
        var colaborador = await EnsureRoleAsync("colaborador", "Colaborador");
        var rrhh = await EnsureRoleAsync("rrhh", "Recursos Humanos");
        var admin = await EnsureRoleAsync("admin", "Administrador");

        await EnsureUserAsync(AdminId, "admin@example.com", "Administrador DEMO", admin, "PLU-2026-0001");
        await EnsureUserAsync(RrhhId, "rrhh@example.com", "Laura RRHH DEMO", rrhh, "PLU-2026-0002");
        await EnsureUserAsync(EmployeeId, "mariana.lopez@example.com", "Mariana López DEMO", colaborador, "PLU-2026-0003");
    }

    private async Task<Guid> EnsureRoleAsync(string name, string description)
    {
        var existing = await _db.Roles.FirstOrDefaultAsync(r => r.Name == name);
        if (existing is not null)
        {
            return existing.Id;
        }

        var role = new Role { Id = Guid.NewGuid(), Name = name, Description = description, CreatedAt = DateTimeOffset.UtcNow };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role.Id;
    }

    private async Task EnsureUserAsync(Guid id, string email, string fullName, Guid roleId, string employeeCode)
    {
        if (await _db.Users.AnyAsync(u => u.Id == id))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        _db.Users.Add(new User
        {
            Id = id,
            Email = email,
            PasswordHash = _hasher.Hash(DemoPassword),
            FullName = fullName,
            RoleId = roleId,
            EmployeeCode = employeeCode,
            IsActive = true,
            MustChangePassword = false,
            CreatedAt = now,
            UpdatedAt = now
        });
        await _db.SaveChangesAsync();
    }
}
