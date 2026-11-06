using System.Net.Http.Json;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Ports;

namespace TransactionsService.Infrastructure.HttpClients;

// Adaptador HTTP para comunicación síncrona con ProductsService
public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;

    public ProductServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Consultar producto por Id en ProductsService
    public async Task<ProductInfoDto?> GetProductByIdAsync(Guid productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/products/{productId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProductInfoDto>();
        }
        catch
        {
            return null;
        }
    }

    // Actualizar stock en ProductsService (PATCH con fallback a PUT)
    public async Task<bool> UpdateProductStockAsync(Guid productId, int newStock)
    {
        try
        {
            var payload = new UpdateStockRequestDto { Stock = newStock };
            var response = await _httpClient.PatchAsJsonAsync($"api/products/{productId}/stock", payload);

            if (!response.IsSuccessStatusCode)
            {
                response = await _httpClient.PutAsJsonAsync($"api/products/{productId}/stock", payload);
            }

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
