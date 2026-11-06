using TransactionsService.Application.DTOs;
using TransactionsService.Application.Ports;
using TransactionsService.Domain.Entities;
using TransactionsService.Domain.Ports;

namespace TransactionsService.Application.Services;

// Implementación de casos de uso de transacciones (inmutables)
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IProductServiceClient _productClient;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IProductServiceClient productClient)
    {
        _transactionRepository = transactionRepository;
        _productClient = productClient;
    }

    // Listar transacciones con filtros dinámicos (producto, tipo, fechas)
    public async Task<PagedResultDto<TransactionResponseDto>> GetFilteredAsync(
        Guid? productId,
        string? transactionType,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var (items, totalCount) = await _transactionRepository.GetFilteredAsync(
            productId, transactionType, startDate, endDate, page, pageSize);

        // Enriquecer transacciones con información del producto
        var responseItems = new List<TransactionResponseDto>();
        var productCache = new Dictionary<Guid, ProductInfoDto?>();

        foreach (var t in items)
        {
            if (!productCache.TryGetValue(t.ProductId, out var product))
            {
                product = await _productClient.GetProductByIdAsync(t.ProductId);
                productCache[t.ProductId] = product;
            }

            responseItems.Add(new TransactionResponseDto
            {
                Id = t.Id,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType,
                ProductId = t.ProductId,
                ProductName = product?.Name ?? "Producto no disponible",
                ProductCode = product?.Code ?? "N/A",
                ProductStock = product?.Stock ?? 0,
                Quantity = t.Quantity,
                UnitPrice = t.UnitPrice,
                TotalPrice = t.TotalPrice,
                Detail = t.Detail,
                CreatedAt = t.CreatedAt
            });
        }

        return new PagedResultDto<TransactionResponseDto>
        {
            Items = responseItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // Obtener transacción por Id
    public async Task<TransactionResponseDto?> GetByIdAsync(Guid id)
    {
        var t = await _transactionRepository.GetByIdAsync(id);
        if (t is null) return null;

        var product = await _productClient.GetProductByIdAsync(t.ProductId);

        return new TransactionResponseDto
        {
            Id = t.Id,
            TransactionDate = t.TransactionDate,
            TransactionType = t.TransactionType,
            ProductId = t.ProductId,
            ProductName = product?.Name,
            ProductCode = product?.Code,
            ProductStock = product?.Stock ?? 0,
            Quantity = t.Quantity,
            UnitPrice = t.UnitPrice,
            TotalPrice = t.TotalPrice,
            Detail = t.Detail,
            CreatedAt = t.CreatedAt
        };
    }

    // Registrar compra o venta
    public async Task<TransactionResponseDto> CreateAsync(TransactionCreateDto dto)
    {
        // 1. Consultar el producto en ProductsService (comunicación síncrona REST)
        var product = await _productClient.GetProductByIdAsync(dto.ProductId);
        if (product is null)
        {
            throw new KeyNotFoundException($"El producto con Id '{dto.ProductId}' no existe.");
        }

        // 2. Normalizar tipo y validar stock
        var type = char.ToUpper(dto.TransactionType[0]) + dto.TransactionType[1..].ToLower();
        int newStock;

        if (type == "Venta")
        {
            // Regla compleja: No permitir vender más stock del disponible
            if (product.Stock < dto.Quantity)
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente. El producto '{product.Name}' tiene {product.Stock} unidades disponibles y se intentó vender {dto.Quantity}.");
            }

            newStock = product.Stock - dto.Quantity;
        }
        else if (type == "Compra")
        {
            newStock = product.Stock + dto.Quantity;
        }
        else
        {
            throw new ArgumentException("El tipo de transacción debe ser 'Compra' o 'Venta'.");
        }

        // 3. Ajustar el stock en ProductsService síncronamente
        var stockUpdated = await _productClient.UpdateProductStockAsync(dto.ProductId, newStock);
        if (!stockUpdated)
        {
            throw new InvalidOperationException("No se pudo actualizar el stock en el microservicio de productos.");
        }

        // 4. Determinar precio unitario (si no se envía o es 0, toma el precio actual del producto)
        var finalUnitPrice = (dto.UnitPrice.HasValue && dto.UnitPrice.Value > 0)
            ? dto.UnitPrice.Value
            : product.Price;

        // 5. Guardar la transacción en la base de datos
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            TransactionDate = DateTime.UtcNow,
            TransactionType = type,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitPrice = finalUnitPrice,
            TotalPrice = dto.Quantity * finalUnitPrice,
            Detail = dto.Detail?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _transactionRepository.CreateAsync(transaction);

        return new TransactionResponseDto
        {
            Id = created.Id,
            TransactionDate = created.TransactionDate,
            TransactionType = created.TransactionType,
            ProductId = created.ProductId,
            ProductName = product.Name,
            ProductCode = product.Code,
            ProductStock = newStock,
            Quantity = created.Quantity,
            UnitPrice = created.UnitPrice,
            TotalPrice = created.TotalPrice,
            Detail = created.Detail,
            CreatedAt = created.CreatedAt
        };
    }
}
