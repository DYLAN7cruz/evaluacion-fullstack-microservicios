namespace ProductsService.Domain.Entities;

// Entidad de producto
public class Product
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Product() { }

    public Product(string code, string name, string? description, string category, string? imageUrl, decimal price, int stock)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Description = description;
        Category = category;
        ImageUrl = imageUrl;
        Price = price;
        Stock = stock;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
