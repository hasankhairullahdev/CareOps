namespace BillingService.Domain.Entities;

public class ServiceTariff
{
    public Guid Id { get; private set; }
    public string ServiceName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private ServiceTariff() { }

    public static ServiceTariff Create(string serviceName, string category, decimal price, string? description = null)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        return new ServiceTariff
        {
            Id = Guid.NewGuid(),
            ServiceName = serviceName,
            Category = category,
            Price = Math.Round(price, 2),
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string serviceName, string category, decimal price, string? description, bool isActive)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        ServiceName = serviceName;
        Category = category;
        Price = Math.Round(price, 2);
        Description = description;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}
