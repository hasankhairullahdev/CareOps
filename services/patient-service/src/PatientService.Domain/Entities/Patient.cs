namespace PatientService.Domain.Entities;

public class Patient
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public string Gender { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string MedicalRecordNumber { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Patient() { }

    public static Patient Create(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string gender,
        string phoneNumber,
        string email,
        string address,
        string medicalRecordNumber)
    {
        return new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            PhoneNumber = phoneNumber,
            Email = email,
            Address = address,
            MedicalRecordNumber = medicalRecordNumber,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string gender,
        string phoneNumber,
        string email,
        string address)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        PhoneNumber = phoneNumber;
        Email = email;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }
}
