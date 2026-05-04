using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers;

[ApiController]
[Route("")]
public class ProductController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _context;

    public ProductController(HttpClient httpClient, AppDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var products = _context.Products.ToList();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var product = _context.Products.Find(id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var content = JsonSerializer.Serialize($"Product created: {product.Name}");

        await _httpClient.PostAsync(
            "http://localhost:5039/logs",
            new StringContent(content, Encoding.UTF8, "application/json")
        );

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}