namespace ProductService.Application.Events;

public class LogEvent
{
    public string ServiceName { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? UserName { get; set; }

    public string? ResourceId { get; set; }
}