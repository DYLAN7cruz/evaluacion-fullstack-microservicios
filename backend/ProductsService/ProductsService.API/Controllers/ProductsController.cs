using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsService.Application.DTOs;
using ProductsService.Application.Ports;

namespace ProductsService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/products (listar con filtros y paginación)
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProductResponseDto>>> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null)
    {
        var result = await _productService.GetFilteredAsync(searchTerm, category, page, pageSize, isActive);
        return Ok(result);
    }

    // GET: api/products/{id} (obtener por Id)
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound(new { message = $"No se encontró el producto con Id '{id}'." });
        }
        return Ok(product);
    }

    // GET: api/products/by-code/{code} (obtener por SKU)
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<ProductResponseDto>> GetByCode(string code)
    {
        var product = await _productService.GetByCodeAsync(code);
        if (product is null)
        {
            return NotFound(new { message = $"No se encontró el producto con código '{code}'." });
        }
        return Ok(product);
    }

    // POST: api/products (crear producto)
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create([FromBody] ProductCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/products/{id} (editar producto)
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponseDto>> Update(Guid id, [FromBody] ProductUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _productService.UpdateAsync(id, dto);
            if (updated is null)
            {
                return NotFound(new { message = $"No se encontró el producto con Id '{id}'." });
            }
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE: api/products/{id} (eliminar producto)
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _productService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"No se encontró el producto con Id '{id}'." });
            }
            return NoContent();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new 
            { 
                message = "No se puede eliminar el producto porque tiene transacciones registradas." 
            });
        }
    }

    // PATCH / PUT: api/products/{id}/stock (actualizar stock)
    [HttpPatch("{id:guid}/stock")]
    [HttpPut("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var success = await _productService.UpdateStockAsync(id, dto.Stock);
            return Ok(new { success, stock = dto.Stock, message = "Stock actualizado correctamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
