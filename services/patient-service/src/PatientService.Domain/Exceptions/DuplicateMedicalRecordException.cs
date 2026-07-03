namespace PatientService.Domain.Exceptions;

public sealed class DuplicateMedicalRecordException : Exception
{
    public DuplicateMedicalRecordException(string medicalRecordNumber)
        : base($"A patient with medical record number '{medicalRecordNumber}' already exists.")
    {
    }
}
