using System.ComponentModel.DataAnnotations;

namespace ProductsService.Application.DTOs;

// DTO para crear producto
public class ProductCreateDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [MaxLength(70)]
    public string Category { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    [Range(0, 9999999.99, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock inicial no puede ser negativo.")]
    public int Stock { get; set; }
}

// DTO para actualizar producto
public class ProductUpdateDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [MaxLength(70)]
    public string Category { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    [Range(0, 9999999.99, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    public bool IsActive { get; set; } = true;
}

// DTO de respuesta para frontend
public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// DTO para actualizar stock
public class UpdateStockDto
{
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }
}
