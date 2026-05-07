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
    public async Task<IActionResult> Create([FromBody] LogRequest request)
    {
        var level = NormalizeLevel(request.Level);

        var log = new Log
        {
            ServiceName = request.ServiceName,
            EventType = request.EventType,
            Level = level,
            Message = request.Message,
            UserName = request.UserName,
            ResourceId = request.ResourceId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();

        WriteStructuredLog(level, log);

        return Ok();
    }

    private static string NormalizeLevel(string level)
    {
        return level.ToUpperInvariant() switch
        {
            "WARNING" => "WARNING",
            "ERROR" => "ERROR",
            "CRITICAL" => "CRITICAL",
            _ => "INFO"
        };
    }

    private void WriteStructuredLog(string level, Log log)
    {
        switch (level)
        {
            case "WARNING":
                _logger.LogWarning(
                    "LogReceived {@LogData}",
                    log);
                break;

            case "ERROR":
                _logger.LogError(
                    "LogReceived {@LogData}",
                    log);
                break;

            case "CRITICAL":
                _logger.LogCritical(
                    "LogReceived {@LogData}",
                    log);
                break;

            default:
                _logger.LogInformation(
                    "LogReceived {@LogData}",
                    log);
                break;
        }
    }
}