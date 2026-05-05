namespace ProductService.Application.Handlers;

using ProductService.Application.Interfaces;
using ProductService.Domain;
using ProductService.Application.Queries;

public class GetProductByIdHandler
{
    private readonly IProductRepository _repository;

    public GetProductByIdHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Product?> Handle(GetProductByIdQuery query)
    {
        return await _repository.GetByIdAsync(query.Id);
    }
}