using Microsoft.AspNetCore.Mvc;

namespace LogService.Controllers;

[ApiController]
[Route("logs")]
public class LogController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] string message)
    {
        Console.WriteLine($"LOG: {message}");

        return Ok();
    }
}