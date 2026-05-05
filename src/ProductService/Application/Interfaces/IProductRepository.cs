namespace ProductService.Application.Interfaces;

using ProductService.Domain;

public interface IProductRepository
{
    Task AddAsync(Product product);
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task UpdateAsync(Product product);
}