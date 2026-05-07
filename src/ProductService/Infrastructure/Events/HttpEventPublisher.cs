namespace ProductService.Infrastructure.Events;

using System.Text;
using System.Text.Json;
using ProductService.Application.Events;

public class HttpEventPublisher : IEventPublisher
{
    private readonly HttpClient _httpClient;

    public HttpEventPublisher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task PublishAsync(LogEvent logEvent)
    {
        await _httpClient.PostAsJsonAsync("create", logEvent);
    }
}