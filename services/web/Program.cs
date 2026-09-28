using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PluriOne.Shared;
using PluriOne.Shared.Auth;
using PluriOne.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
});

builder.Services.AddControllers();

builder.Services.AddPluriOneAuth(builder.Configuration);

// El BFF autentica usando la cookie HttpOnly `access_token`.
builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(
                    new PluriOne.Shared.Contracts.ApiError("unauthorized", "Autenticación requerida.")));
            }
            else
            {
                context.Response.Redirect("/Account/Login?returnUrl=" +
                    Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
            }
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                new PluriOne.Shared.Contracts.ApiError("forbidden", "No tiene permisos.")));
        }
    };
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
    options.Cookie.Name = "pluri.antiforgery";
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
});

builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<ForwardAuthHandler>();
builder.Services.AddScoped<AuthFlowService>();
builder.Services.AddScoped<Gateway>();

builder.Services.AddHttpClient("auth", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["AUTH_SERVICE_URL"] ?? "http://localhost:8001");
}).AddHttpMessageHandler<ForwardAuthHandler>();

builder.Services.AddHttpClient("onboarding", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ONBOARDING_SERVICE_URL"] ?? "http://localhost:8002");
}).AddHttpMessageHandler<ForwardAuthHandler>();

builder.Services.AddHttpClient("hr", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["HR_SERVICE_URL"] ?? "http://localhost:8003");
}).AddHttpMessageHandler<ForwardAuthHandler>();

builder.Services.AddHttpClient("ai", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["AI_SERVICE_URL"] ?? "http://localhost:8004");
    c.Timeout = TimeSpan.FromMinutes(2);
}).AddHttpMessageHandler<ForwardAuthHandler>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCookiePolicy();
app.UseStaticFiles();
app.UseRouting();

app.UseMiddleware<PluriOne.Web.Middleware.RefreshMiddleware>();

app.UsePluriOneCommonMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "web" }));

app.Run();

public partial class Program;
