namespace PharmacyService.Domain.Entities;

public class Supplier
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ContactPerson { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private Supplier() { }

    public static Supplier Create(string name, string? contactPerson = null,
        string? phone = null, string? email = null, string? address = null)
    {
        return new Supplier
        {
            Id = Guid.NewGuid(),
            Name = name,
            ContactPerson = contactPerson,
            Phone = phone,
            Email = email,
            Address = address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? contactPerson, string? phone, string? email, string? address, bool isActive)
    {
        Name = name;
        ContactPerson = contactPerson;
        Phone = phone;
        Email = email;
        Address = address;
        IsActive = isActive;
    }
}
