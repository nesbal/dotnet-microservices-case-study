namespace ProductService.Application.Handlers;

using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain;
using ProductService.Application.Events;
using Microsoft.Extensions.Caching.Distributed;

public class CreateProductHandler
{
    private readonly IProductRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IDistributedCache _cache;

    public CreateProductHandler(
        IProductRepository repository,
        IEventPublisher eventPublisher,
        IDistributedCache cache)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _cache = cache;
    }

    public async Task<Product> Handle(CreateProductCommand command, string username)
    {
        var product = new Product
        {
            Name = command.Name,
            Price = command.Price,
            OwnerUsername = username
        };

        await _repository.AddAsync(product);

        await _cache.RemoveAsync("products_all");

        await _eventPublisher.PublishAsync($"Product created: {product.Name}");
        
        return product;
    }
}