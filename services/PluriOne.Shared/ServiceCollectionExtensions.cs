using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PluriOne.Shared.Auth;
using PluriOne.Shared.Data;
using PluriOne.Shared.Observability;

namespace PluriOne.Shared;

public static class ServiceCollectionExtensions
{
    /// <summary>Registra autenticación JWT y el contexto de usuario actual.</summary>
    public static IServiceCollection AddPluriOneAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<JwtTokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new JwtTokenService(
                    Microsoft.Extensions.Options.Options.Create(jwtOptions)).ValidationParameters();
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(
                            new Contracts.ApiError("unauthorized", "Autenticación requerida.")));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(
                            new Contracts.ApiError("forbidden", "No tiene permisos para esta operación.")));
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>Registra un DbContext con el interceptor de contexto de usuario para RLS.</summary>
    public static IServiceCollection AddPluriOneDbContext<TContext>(
        this IServiceCollection services, string connectionString)
        where TContext : DbContext
    {
        services.AddSingleton<PostgresUserContextInterceptor>();
        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<PostgresUserContextInterceptor>());
        });
        return services;
    }

    public static IApplicationBuilder UsePluriOneCommonMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>Debe registrarse DESPUÉS de UseAuthentication para que el contexto
    /// de usuario (claims JWT) esté disponible para el interceptor de RLS.</summary>
    public static IApplicationBuilder UsePluriOneUserContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<DbUserContextMiddleware>();
    }
}
