using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayReverseProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        return services;
    }

    public static IServiceCollection AddGatewayRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromSeconds(10);
            });
        });

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
}