namespace ProductService.Application.Handlers;

using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain;
using ProductService.Application.Events;
using Microsoft.Extensions.Caching.Distributed;

public class UpdateProductHandler
{
    private readonly IProductRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IDistributedCache _cache;

    public UpdateProductHandler(
        IProductRepository repository,
        IEventPublisher eventPublisher,
        IDistributedCache cache)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _cache = cache;
    }

    public async Task<bool> Handle(UpdateProductCommand command)
    {
        var product = await _repository.GetByIdAsync(command.Id);

        if (product == null)
            return false;

        product.Name = command.Name;
        product.Price = command.Price;

        await _repository.UpdateAsync(product);

        await _cache.RemoveAsync("products_all");

        await _eventPublisher.PublishAsync(new LogEvent
        {
            ServiceName = "ProductService",
            EventType = "ProductUpdated",
            Level = "INFO",
            Message = $"Product updated: {product.Name}",
            UserName = product.OwnerUsername,
            ResourceId = product.Id.ToString()
        });

        return true;
    }
}