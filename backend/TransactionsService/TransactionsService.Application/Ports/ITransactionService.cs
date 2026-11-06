using TransactionsService.Application.DTOs;

namespace TransactionsService.Application.Ports;

// Casos de uso de transacciones
public interface ITransactionService
{
    Task<PagedResultDto<TransactionResponseDto>> GetFilteredAsync(
        Guid? productId,
        string? transactionType,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize);

    Task<TransactionResponseDto?> GetByIdAsync(Guid id);
    Task<TransactionResponseDto> CreateAsync(TransactionCreateDto dto);
    Task<TransactionResponseDto?> UpdateAsync(Guid id, TransactionUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
