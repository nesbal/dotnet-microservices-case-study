using System.Net.Http.Json;
using AuthService.Models;

namespace AuthService.Services;

public class HttpLogEventPublisher : ILogEventPublisher
{
    private readonly HttpClient _httpClient;

    public HttpLogEventPublisher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task PublishAsync(LogEvent logEvent)
    {
        await _httpClient.PostAsJsonAsync("http://logservice/create", logEvent);
    }
}