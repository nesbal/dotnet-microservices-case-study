using System.Text;
using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey = configuration["JWT_KEY"]
            ?? throw new Exception("JWT_KEY is missing");

        var issuer = configuration["JWT_ISSUER"]
            ?? throw new Exception("JWT_ISSUER is missing");

        var audience = configuration["JWT_AUDIENCE"]
            ?? throw new Exception("JWT_AUDIENCE is missing");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            })
            .AddJwtBearer("Bearer", options =>
            {
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey));

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key
                };
            });

        return services;
    }

    public static IServiceCollection AddLogPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var logServiceUrl = configuration["LOG_SERVICE_URL"]
                            ?? throw new Exception("LOG_SERVICE_URL is missing");

        services.AddHttpClient<ILogEventPublisher, HttpLogEventPublisher>(client =>
        {
            client.BaseAddress = new Uri(logServiceUrl);
        });

        return services;
    }
}