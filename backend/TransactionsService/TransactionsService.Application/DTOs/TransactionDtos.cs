using System.ComponentModel.DataAnnotations;

namespace TransactionsService.Application.DTOs;

// DTO para registrar transacción
public class TransactionCreateDto
{
    [Required(ErrorMessage = "El tipo de transacción es obligatorio.")]
    [RegularExpression("^(Compra|Venta)$", ErrorMessage = "El tipo debe ser 'Compra' o 'Venta'.")]
    public string TransactionType { get; set; } = string.Empty;

    [Required(ErrorMessage = "El identificador de producto es obligatorio.")]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Quantity { get; set; }

    [Range(0, 9999999.99, ErrorMessage = "El precio unitario debe ser mayor o igual a 0.")]
    public decimal? UnitPrice { get; set; }

    public string? Detail { get; set; }
}

// DTO de respuesta para frontend con datos enriquecidos del producto
public class TransactionResponseDto
{
    public Guid Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public int ProductStock { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Detail { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Datos básicos del producto obtenidos del microservicio de productos
public class ProductInfoDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
}

// DTO para enviar nuevo stock al microservicio de productos
public class UpdateStockRequestDto
{
    public int Stock { get; set; }
}
