using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Entities;

namespace ProductsService.Infrastructure.Persistence;

// DbContext de Entity Framework para esquema 'catalog'
public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapeo de tabla catalog.products
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products", schema: "catalog");

            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id");

            entity.Property(p => p.Code)
                  .HasColumnName("code")
                  .HasMaxLength(50)
                  .IsRequired();
            entity.HasIndex(p => p.Code).IsUnique();

            entity.Property(p => p.Name)
                  .HasColumnName("name")
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(p => p.Description)
                  .HasColumnName("description");

            entity.Property(p => p.Category)
                  .HasColumnName("category")
                  .HasMaxLength(70)
                  .IsRequired();

            entity.Property(p => p.ImageUrl)
                  .HasColumnName("image_url");

            entity.Property(p => p.Price)
                  .HasColumnName("price")
                  .HasColumnType("numeric(12,2)")
                  .IsRequired();

            entity.Property(p => p.Stock)
                  .HasColumnName("stock")
                  .IsRequired();

            entity.Property(p => p.IsActive)
                  .HasColumnName("is_active")
                  .HasDefaultValue(true)
                  .IsRequired();

            entity.Property(p => p.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(p => p.UpdatedAt)
                  .HasColumnName("updated_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
