using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductService.Models;

namespace ProductService.Controllers;

[ApiController]
[Route("")]
public class ProductController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public ProductController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

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
    
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product) {
        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);
        var content = JsonSerializer.Serialize($"Product created: {product.Name}");

        await _httpClient.PostAsync(
            "http://localhost:5039/logs",
            new StringContent(content, Encoding.UTF8, "application/json")
        );
        return CreatedAtAction(nameof(GetAll), new { id = product.Id }, product);
    }
}