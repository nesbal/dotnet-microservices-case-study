namespace ProductService.Application.Handlers;

using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain;
using ProductService.Application.Events;

public class CreateProductHandler
{
    private readonly IProductRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public CreateProductHandler(
        IProductRepository repository,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Product> Handle(CreateProductCommand command)
    {
        var product = new Product
        {
            Name = command.Name,
            Price = command.Price
        };

        await _repository.AddAsync(product);

        await _eventPublisher.PublishAsync($"Product created: {product.Name}");

        return product;
    }
}