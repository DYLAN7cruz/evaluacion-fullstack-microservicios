using ProductsService.Domain.Entities;

namespace ProductsService.Domain.Ports;

// Contrato para persistencia de productos
public interface IProductRepository
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredAsync(string? searchTerm, string? category, int page, int pageSize, bool? isActive = null);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetByCodeAsync(string code);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateStockAsync(Guid id, int newStock);
    Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null);
}
