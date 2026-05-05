using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductService.Domain;
using ProductService.Application.Interfaces;
using ProductService.Application.Handlers;
using ProductService.Application.Commands;

namespace ProductService.Controllers;

[ApiController]
[Route("")]
public class ProductController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IProductRepository _repository;
    private readonly CreateProductHandler _createHandler;

    public ProductController(HttpClient httpClient, CreateProductHandler createHandler)
    {
        _httpClient = httpClient;
        _createHandler = createHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _repository.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _repository.GetByIdAsync(id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var product = await _createHandler.Handle(command);

        var content = JsonSerializer.Serialize($"Product created: {product.Name}");

        await _httpClient.PostAsync(
            "http://localhost:5039/logs",
            new StringContent(content, Encoding.UTF8, "application/json")
        );

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}