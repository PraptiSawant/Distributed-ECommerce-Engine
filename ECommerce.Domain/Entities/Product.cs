namespace ECommerce.Domain.Entities;

public class Product
{
    // High-performance UUID for modern distributed systems
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    // Private constructor required by Entity Framework Core
    private Product() { }

    // Clean Domain Driven Design (DDD) encapsulation pattern
    public Product(Guid id, string name, string description, decimal price, int stockQuantity, string imageUrl, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));

        if (price < 0)
            throw new ArgumentException("Product price cannot be negative.", nameof(price));

        Id = id;
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        ImageUrl = imageUrl;
        Category = category;
    }

    // Business Logic Method: Updates inventory counts safely
    public void UpdateStock(int quantity)
    {
        if (StockQuantity + quantity < 0)
        {
            throw new InvalidOperationException($"Insufficient stock available for product: {Name}");
        }
        StockQuantity += quantity;
    }
}

