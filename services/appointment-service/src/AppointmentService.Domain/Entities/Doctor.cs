namespace AppointmentService.Domain.Entities;

public class Doctor
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Specialization { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public string Schedule { get; private set; } = string.Empty; // e.g. "Mon-Fri 08:00-16:00"

    private Doctor() { }

    public static Doctor Create(string name, string specialization, string licenseNumber, string schedule)
    {
        return new Doctor
        {
            Id = Guid.NewGuid(),
            Name = name,
            Specialization = specialization,
            LicenseNumber = licenseNumber,
            Schedule = schedule
        };
    }
}
