namespace ProductService.Application.Handlers;

using ProductService.Application.Interfaces;
using ProductService.Domain;

public class GetAllProductsHandler
{
    private readonly IProductRepository _repository;

    public GetAllProductsHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Product>> Handle()
    {
        return await _repository.GetAllAsync();
    }
}