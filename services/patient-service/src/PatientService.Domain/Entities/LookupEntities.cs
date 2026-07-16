namespace PatientService.Domain.Entities;

public class BloodType
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty; // A, B, AB, O, A+, A-, etc.

    private BloodType() { }

    public BloodType(int id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class AllergyType
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private AllergyType() { }

    public static AllergyType Create(string name, string? description = null)
    {
        return new AllergyType
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
