using AuthService.Models;

namespace AuthService.Services;

public interface ILogEventPublisher
{
    Task PublishAsync(LogEvent logEvent);
}