using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Ports;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Infrastructure.Repositories;

// Repositorio de productos con PostgreSQL
public class ProductRepository : IProductRepository
{
    private readonly ProductsDbContext _context;

    public ProductRepository(ProductsDbContext context)
    {
        _context = context;
    }

    // Listado con filtros y paginación
    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredAsync(
        string? searchTerm, 
        string? category, 
        int page, 
        int pageSize, 
        bool? isActive = null)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.ToLower() == category.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Code.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product?> GetByCodeAsync(string code)
    {
        var normalizedCode = code.Trim().ToUpper();
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == normalizedCode);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStockAsync(Guid id, int newStock)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return false;
        }

        product.Stock = newStock;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null)
    {
        var normalizedCode = code.Trim().ToUpper();
        var query = _context.Products.AsNoTracking().Where(p => p.Code == normalizedCode);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
