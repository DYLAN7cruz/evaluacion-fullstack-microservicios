using TransactionsService.Domain.Entities;

namespace TransactionsService.Domain.Ports;

// Contrato para persistencia de transacciones
public interface ITransactionRepository
{
    Task<(IEnumerable<Transaction> Items, int TotalCount)> GetFilteredAsync(
        Guid? productId,
        string? transactionType,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize);

    Task<Transaction?> GetByIdAsync(Guid id);
    Task<Transaction> CreateAsync(Transaction transaction);
    Task<Transaction?> UpdateAsync(Transaction transaction);
    Task<bool> DeleteAsync(Guid id);
}
