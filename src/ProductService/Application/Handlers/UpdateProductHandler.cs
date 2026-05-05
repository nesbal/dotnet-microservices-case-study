namespace ProductService.Application.Handlers;

using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain;

public class UpdateProductHandler
{
    private readonly IProductRepository _repository;

    public UpdateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateProductCommand command)
    {
        var product = await _repository.GetByIdAsync(command.Id);

        if (product == null)
            return false;

        product.Name = command.Name;
        product.Price = command.Price;

        await _repository.UpdateAsync(product);

        return true;
    }
}