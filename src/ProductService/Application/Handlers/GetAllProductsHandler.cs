namespace ProductService.Application.Handlers;

using ProductService.Application.Interfaces;
using ProductService.Domain;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public class GetAllProductsHandler
{
    private readonly IProductRepository _repository;
    private readonly IDistributedCache _cache;

    public GetAllProductsHandler(
        IProductRepository repository,
        IDistributedCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<Product>> Handle()
    {
        var cacheKey = "products_all";

        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<List<Product>>(cachedData)!;
        }

        var products = await _repository.GetAllAsync();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(products),
            options
        );

        return products;
    }
}