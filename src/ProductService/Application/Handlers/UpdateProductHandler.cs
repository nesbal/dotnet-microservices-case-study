namespace ProductService.Application.Handlers;

using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain;
using ProductService.Application.Events;

public class UpdateProductHandler
{
    private readonly IProductRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public UpdateProductHandler(
        IProductRepository repository,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(UpdateProductCommand command)
    {
        var product = await _repository.GetByIdAsync(command.Id);

        if (product == null)
            return false;

        product.Name = command.Name;
        product.Price = command.Price;

        await _repository.UpdateAsync(product);

        await _eventPublisher.PublishAsync($"Product updated: {product.Name}");

        return true;
    }
}