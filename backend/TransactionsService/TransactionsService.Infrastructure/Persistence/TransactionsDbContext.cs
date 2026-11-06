using Microsoft.EntityFrameworkCore;
using TransactionsService.Domain.Entities;

namespace TransactionsService.Infrastructure.Persistence;

// DbContext para el esquema 'inventory'
public class TransactionsDbContext : DbContext
{
    public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions", schema: "inventory");

            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id");

            entity.Property(t => t.TransactionDate)
                  .HasColumnName("transaction_date")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(t => t.TransactionType)
                  .HasColumnName("transaction_type")
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(t => t.ProductId)
                  .HasColumnName("product_id")
                  .IsRequired();

            entity.Property(t => t.Quantity)
                  .HasColumnName("quantity")
                  .IsRequired();

            entity.Property(t => t.UnitPrice)
                  .HasColumnName("unit_price")
                  .HasColumnType("numeric(12,2)")
                  .IsRequired();

            entity.Property(t => t.TotalPrice)
                  .HasColumnName("total_price")
                  .HasColumnType("numeric(12,2)")
                  .IsRequired();

            entity.Property(t => t.Detail)
                  .HasColumnName("detail");

            entity.Property(t => t.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
