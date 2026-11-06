using TransactionsService.Application.DTOs;

namespace TransactionsService.Application.Ports;

// Puerto de salida para comunicación síncrona con ProductsService
public interface IProductServiceClient
{
    Task<ProductInfoDto?> GetProductByIdAsync(Guid productId);
    Task<bool> UpdateProductStockAsync(Guid productId, int newStock);
}
