using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductService.Application.Events;
using ProductService.Application.Handlers;
using ProductService.Application.Interfaces;
using ProductService.Data;
using ProductService.Infrastructure.Events;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }

    public static IServiceCollection AddProductApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<CreateProductHandler>();
        services.AddScoped<UpdateProductHandler>();
        services.AddScoped<GetAllProductsHandler>();
        services.AddScoped<GetProductByIdHandler>();

        return services;
    }

    public static IServiceCollection AddProductLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var logServiceUrl = configuration["LOG_SERVICE_URL"]
                            ?? throw new Exception("LOG_SERVICE_URL is missing");

        services.AddHttpClient<IEventPublisher, HttpEventPublisher>(client =>
        {
            client.BaseAddress = new Uri(logServiceUrl);
        });

        return services;
    }

    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnection = configuration["Redis:ConnectionString"]
                              ?? throw new Exception("Redis connection string is missing");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
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
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
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