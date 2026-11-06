using Microsoft.EntityFrameworkCore;
using TransactionsService.Domain.Entities;
using TransactionsService.Domain.Ports;
using TransactionsService.Infrastructure.Persistence;

namespace TransactionsService.Infrastructure.Repositories;

// Repositorio de transacciones con PostgreSQL
public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionsDbContext _context;

    public TransactionRepository(TransactionsDbContext context)
    {
        _context = context;
    }

    // Listado con filtros y paginación
    public async Task<(IEnumerable<Transaction> Items, int TotalCount)> GetFilteredAsync(
        Guid? productId,
        string? transactionType,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize)
    {
        IQueryable<Transaction> query = _context.Transactions.AsNoTracking();

        // Filtro por producto
        if (productId.HasValue && productId.Value != Guid.Empty)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        // Filtro por tipo de transacción (Compra o Venta)
        if (!string.IsNullOrWhiteSpace(transactionType))
        {
            query = query.Where(t => t.TransactionType.ToLower() == transactionType.Trim().ToLower());
        }

        // Filtro por rango de fechas
        if (startDate.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(t => t.TransactionDate >= startUtc);
        }

        if (endDate.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(t => t.TransactionDate <= endUtc);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions.FindAsync(id);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }
}
