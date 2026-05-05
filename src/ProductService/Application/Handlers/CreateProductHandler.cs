namespace ProductService.Application.Handlers;

using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain;

public class CreateProductHandler
{
    private readonly IProductRepository _repository;

    public CreateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Product> Handle(CreateProductCommand command)
    {
        var product = new Product
        {
            Name = command.Name,
            Price = command.Price
        };

        await _repository.AddAsync(product);

        return product;
    }
}