using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Handlers;
using ProductService.Application.Commands;
using Microsoft.AspNetCore.Authorization;
using ProductService.Application.Queries;

namespace ProductService.Controllers;

[ApiController]
[Route("")]
public class ProductController : ControllerBase
{
    private readonly CreateProductHandler _createHandler;
    private readonly UpdateProductHandler _updateHandler;
    private readonly GetAllProductsHandler _getAllHandler;
    private readonly GetProductByIdHandler _getByIdHandler;

    public ProductController(
        CreateProductHandler createHandler,
        UpdateProductHandler updateHandler,
        GetAllProductsHandler getAllHandler,
        GetProductByIdHandler getByIdHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _getAllHandler.Handle();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _getByIdHandler.Handle(new GetProductByIdQuery { Id = id });

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var product = await _createHandler.Handle(command);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
    
    [Authorize(Roles = "Admin")]
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