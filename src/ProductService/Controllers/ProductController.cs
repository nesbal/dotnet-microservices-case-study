using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductService.Domain;
using ProductService.Application.Interfaces;
using ProductService.Application.Handlers;
using ProductService.Application.Commands;
using Microsoft.AspNetCore.Authorization;
using ProductService.Application.Commands;
using ProductService.Application.Handlers;
using ProductService.Application.Events;

namespace ProductService.Controllers;

[ApiController]
[Route("")]
public class ProductController : ControllerBase
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IProductRepository _repository;
    private readonly CreateProductHandler _createHandler;
    private readonly UpdateProductHandler _updateHandler;

    public ProductController(
        IEventPublisher eventPublisher,
        IProductRepository repository,
        CreateProductHandler createHandler,
        UpdateProductHandler updateHandler)
    {
        _eventPublisher = eventPublisher;
        _repository = repository;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
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

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
    
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        var success = await _updateHandler.Handle(command);

        if (!success)
            return NotFound();

        return NoContent();
    }
}