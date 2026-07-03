namespace PharmacyService.Domain.Entities;

public class Medicine
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string GenericName { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public int StockQuantity { get; private set; }
    public int MinimumStock { get; private set; }
    public decimal Price { get; private set; }
    public DateOnly ExpiryDate { get; private set; }

    private Medicine() { }

    public static Medicine Create(
        string name, string genericName, string category,
        string unit, int stockQuantity, int minimumStock,
        decimal price, DateOnly expiryDate)
    {
        return new Medicine
        {
            Id = Guid.NewGuid(),
            Name = name,
            GenericName = genericName,
            Category = category,
            Unit = unit,
            StockQuantity = stockQuantity,
            MinimumStock = minimumStock,
            Price = price,
            ExpiryDate = expiryDate
        };
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        StockQuantity += quantity;
    }

    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (StockQuantity < quantity)
            throw new PharmacyService.Domain.Exceptions.InsufficientStockException(Id, Name, quantity, StockQuantity);
        StockQuantity -= quantity;
    }

    public bool IsLowStock => StockQuantity <= MinimumStock;
    public bool IsExpiringSoon => ExpiryDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
}
