using LogService.Data;
using LogService.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogService.Controllers;

[ApiController]
[Route("logs")]
public class LogController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<LogController> _logger;

    public LogController(AppDbContext context, ILogger<LogController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] string message)
    {
        _context.Logs.Add(new Log
        {
            Message = message,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(message) &&
            message.Contains("error", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Error event: {Message}", message);
        }
        else
        {
            _logger.LogInformation("Product event received: {Message}", message);
        }

        return Ok();
    }
}