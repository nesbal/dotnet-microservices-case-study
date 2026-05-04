using Microsoft.AspNetCore.Mvc;
using ProductService.Models;

namespace ProductService.Controllers;

[ApiController]
[Route("")]
public class ProductController : ControllerBase
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Keyboard", Price = 100 },
        new Product { Id = 2, Name = "Mouse", Price = 50 }
    };

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_products);
    }
}