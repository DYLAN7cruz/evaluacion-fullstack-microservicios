using ProductsService.Application.DTOs;

namespace ProductsService.Application.Ports;

// Casos de uso de productos
public interface IProductService
{
    Task<PagedResultDto<ProductResponseDto>> GetFilteredAsync(string? searchTerm, string? category, int page, int pageSize, bool? isActive = null);
    Task<ProductResponseDto?> GetByIdAsync(Guid id);
    Task<ProductResponseDto?> GetByCodeAsync(string code);
    Task<ProductResponseDto> CreateAsync(ProductCreateDto dto);
    Task<ProductResponseDto?> UpdateAsync(Guid id, ProductUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateStockAsync(Guid id, int stock);
}
