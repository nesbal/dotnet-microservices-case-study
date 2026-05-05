namespace ProductService.Application.Events;

public interface IEventPublisher
{
    Task PublishAsync(string message);
}