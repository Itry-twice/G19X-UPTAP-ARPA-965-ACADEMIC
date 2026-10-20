using PluriOne.AuthService.Data;
using PluriOne.AuthService.Services;
using PluriOne.Shared;
using PluriOne.Shared.Auth;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddPluriOneAuth(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("PluriOne")
    ?? throw new InvalidOperationException("Connection string 'PluriOne' no configurada.");

builder.Services.AddPluriOneDbContext<AuthDbContext>(connectionString);

var redisConnection = builder.Configuration["Redis:ConnectionString"]
    ?? throw new InvalidOperationException("Redis:ConnectionString no configurado.");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<LoginRateLimiter>();
builder.Services.AddScoped<AuthManager>();
builder.Services.AddScoped<AdminManager>();
builder.Services.AddScoped<DemoSeeder>();

var app = builder.Build();

app.UsePluriOneCommonMiddleware();
app.UseAuthentication();
app.UsePluriOneUserContext();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "auth-service" }));

// Las cuentas DEMO solo se crean en desarrollo o si se habilita explícitamente
// con SEED_DEMO=true (nunca en producción por defecto).
if (app.Environment.IsDevelopment() ||
    string.Equals(builder.Configuration["SeedDemo"], "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DemoSeeder>();
    await seeder.SeedAsync();
}

app.Run();

public partial class Program;
