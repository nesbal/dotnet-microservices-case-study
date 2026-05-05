namespace ProductService.Application.Commands;

public class CreateProductCommand
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}