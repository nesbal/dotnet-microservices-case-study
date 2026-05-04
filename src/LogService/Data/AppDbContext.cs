using Microsoft.EntityFrameworkCore;
using LogService.Models;

namespace LogService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Log> Logs => Set<Log>();
}