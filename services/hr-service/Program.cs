using PluriOne.HrService.Data;
using PluriOne.HrService.Services;
using PluriOne.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddPluriOneAuth(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("PluriOne")
    ?? throw new InvalidOperationException("Connection string 'PluriOne' no configurada.");

builder.Services.AddPluriOneDbContext<HrDbContext>(connectionString);
builder.Services.AddScoped<HrManager>();

var app = builder.Build();

app.UsePluriOneCommonMiddleware();
app.UseAuthentication();
app.UsePluriOneUserContext();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "hr-service" }));

app.Run();

public partial class Program;
