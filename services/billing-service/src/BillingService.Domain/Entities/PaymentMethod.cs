namespace BillingService.Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private PaymentMethod() { }

    public static PaymentMethod Create(string name, string? description = null)
    {
        return new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
    }
}
