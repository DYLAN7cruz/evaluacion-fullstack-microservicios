namespace TransactionsService.Domain.Entities;

// Entidad de transacción de inventario
public class Transaction
{
    public Guid Id { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string TransactionType { get; set; } = string.Empty; // "Compra" o "Venta"
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Detail { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Transaction() { }

    public Transaction(string transactionType, Guid productId, int quantity, decimal unitPrice, string? detail)
    {
        Id = Guid.NewGuid();
        TransactionDate = DateTime.UtcNow;
        TransactionType = transactionType;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = quantity * unitPrice;
        Detail = detail;
        CreatedAt = DateTime.UtcNow;
    }
}
