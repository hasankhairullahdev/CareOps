namespace PatientService.Domain.ValueObjects;

public sealed class MedicalRecordNumber
{
    public string Value { get; }

    private MedicalRecordNumber(string value) => Value = value;

    public static MedicalRecordNumber Generate()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Random.Shared.Next(1000, 9999);
        return new MedicalRecordNumber($"MRN-{datePart}-{randomPart}");
    }

    public static MedicalRecordNumber From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Medical record number cannot be empty.", nameof(value));
        return new MedicalRecordNumber(value);
    }

    public override string ToString() => Value;
    public static implicit operator string(MedicalRecordNumber mrn) => mrn.Value;
}
