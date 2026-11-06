using ProductsService.Application.DTOs;
using ProductsService.Application.Ports;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Ports;

namespace ProductsService.Application.Services;

// Implementación de casos de uso de productos
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    // Listar con filtros y paginación
    public async Task<PagedResultDto<ProductResponseDto>> GetFilteredAsync(
        string? searchTerm, 
        string? category, 
        int page, 
        int pageSize, 
        bool? isActive = null)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var (items, totalCount) = await _productRepository.GetFilteredAsync(
            searchTerm, category, page, pageSize, isActive);

        return new PagedResultDto<ProductResponseDto>
        {
            Items = items.Select(MapToResponseDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // Obtener por Id
    public async Task<ProductResponseDto?> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product is null ? null : MapToResponseDto(product);
    }

    // Obtener por código SKU
    public async Task<ProductResponseDto?> GetByCodeAsync(string code)
    {
        var product = await _productRepository.GetByCodeAsync(code);
        return product is null ? null : MapToResponseDto(product);
    }

    // Crear producto
    public async Task<ProductResponseDto> CreateAsync(ProductCreateDto dto)
    {
        // Validar código duplicado
        var codeExists = await _productRepository.ExistsCodeAsync(dto.Code);
        if (codeExists)
        {
            throw new InvalidOperationException($"El código '{dto.Code}' ya existe.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = dto.Code.Trim().ToUpper(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Category = dto.Category.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            Price = dto.Price,
            Stock = dto.Stock,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _productRepository.CreateAsync(product);
        return MapToResponseDto(created);
    }

    // Editar producto
    public async Task<ProductResponseDto?> UpdateAsync(Guid id, ProductUpdateDto dto)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return null;
        }

        // Validar que el código no lo use otro producto
        var codeExists = await _productRepository.ExistsCodeAsync(dto.Code, excludeId: id);
        if (codeExists)
        {
            throw new InvalidOperationException($"El código '{dto.Code}' ya está en uso.");
        }

        existing.Code = dto.Code.Trim().ToUpper();
        existing.Name = dto.Name.Trim();
        existing.Description = dto.Description?.Trim();
        existing.Category = dto.Category.Trim();
        existing.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        existing.Price = dto.Price;
        existing.Stock = dto.Stock;
        existing.IsActive = dto.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _productRepository.UpdateAsync(existing);
        return MapToResponseDto(updated);
    }

    // Eliminar producto
    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _productRepository.DeleteAsync(id);
    }

    // Actualizar stock
    public async Task<bool> UpdateStockAsync(Guid id, int stock)
    {
        if (stock < 0)
        {
            throw new InvalidOperationException("El stock no puede ser negativo.");
        }

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            throw new KeyNotFoundException($"No se encontró el producto con Id '{id}'.");
        }

        return await _productRepository.UpdateStockAsync(id, stock);
    }

    // Mapeo a DTO de respuesta
    private static ProductResponseDto MapToResponseDto(Product p) => new()
    {
        Id = p.Id,
        Code = p.Code,
        Name = p.Name,
        Description = p.Description,
        Category = p.Category,
        ImageUrl = p.ImageUrl,
        Price = p.Price,
        Stock = p.Stock,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
