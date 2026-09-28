using PluriOne.OnboardingService.Data;
using PluriOne.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddPluriOneAuth(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("PluriOne")
    ?? throw new InvalidOperationException("Connection string 'PluriOne' no configurada.");

builder.Services.AddPluriOneDbContext<OnboardingDbContext>(connectionString);
builder.Services.AddScoped<PluriOne.OnboardingService.Services.OnboardingManager>();

var app = builder.Build();

app.UsePluriOneCommonMiddleware();
app.UseAuthentication();
app.UsePluriOneUserContext();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "onboarding-service" }));

app.Run();

public partial class Program;
