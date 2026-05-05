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

    public async Task PublishAsync(string message)
    {
        var content = JsonSerializer.Serialize(message);

        await _httpClient.PostAsync(
            "http://localhost:5039/logs",
            new StringContent(content, Encoding.UTF8, "application/json")
        );
    }
}