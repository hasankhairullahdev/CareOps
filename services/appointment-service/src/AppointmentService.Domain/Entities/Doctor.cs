namespace AppointmentService.Domain.Entities;

public class Doctor
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Specialization { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public string Schedule { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private Doctor() { }

    public static Doctor Create(
        string name, string specialization, string licenseNumber,
        string schedule, string? phone = null, string? email = null)
    {
        return new Doctor
        {
            Id = Guid.NewGuid(),
            Name = name,
            Specialization = specialization,
            LicenseNumber = licenseNumber,
            Schedule = schedule,
            Phone = phone,
            Email = email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string specialization, string licenseNumber,
        string schedule, string? phone, string? email, bool isActive)
    {
        Name = name;
        Specialization = specialization;
        LicenseNumber = licenseNumber;
        Schedule = schedule;
        Phone = phone;
        Email = email;
        IsActive = isActive;
    }
}
