using LogService.Data;
using Microsoft.EntityFrameworkCore;

namespace LogService.Extensions;

public static class WebApplicationExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}