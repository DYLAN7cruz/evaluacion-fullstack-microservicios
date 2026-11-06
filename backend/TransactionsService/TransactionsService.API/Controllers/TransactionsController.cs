using Microsoft.AspNetCore.Mvc;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Ports;

namespace TransactionsService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    // GET: api/transactions (listar con filtros de producto, fechas y tipo)
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TransactionResponseDto>>> GetAll(
        [FromQuery] Guid? productId,
        [FromQuery] string? transactionType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _transactionService.GetFilteredAsync(
            productId, transactionType, startDate, endDate, page, pageSize);

        return Ok(result);
    }

    // GET: api/transactions/{id} (obtener detalle por Id)
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionResponseDto>> GetById(Guid id)
    {
        var item = await _transactionService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound(new { message = $"No se encontró la transacción con Id '{id}'." });
        }
        return Ok(item);
    }

    // POST: api/transactions (registrar compra o venta)
    [HttpPost]
    public async Task<ActionResult<TransactionResponseDto>> Create([FromBody] TransactionCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _transactionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/transactions/{id} (editar transacción y reajustar stock)
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TransactionResponseDto>> Update(Guid id, [FromBody] TransactionUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _transactionService.UpdateAsync(id, dto);
            if (updated is null)
            {
                return NotFound(new { message = $"No se encontró la transacción con Id '{id}'." });
            }
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE: api/transactions/{id} (eliminar transacción y revertir stock)
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _transactionService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"No se encontró la transacción con Id '{id}'." });
        }
        return NoContent();
    }
}
