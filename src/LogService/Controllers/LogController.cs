using LogService.Data;
using LogService.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogService.Controllers;

[ApiController]
[Route("logs")]
public class LogController : ControllerBase
{
    private readonly AppDbContext _context;

    public LogController(AppDbContext context)
    {
        _context = context;
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

        Console.WriteLine($"LOG: {message}");

        return Ok();
    }
}