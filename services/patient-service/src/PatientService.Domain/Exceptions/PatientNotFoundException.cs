namespace PatientService.Domain.Exceptions;

public sealed class PatientNotFoundException : Exception
{
    public PatientNotFoundException(Guid patientId)
        : base($"Patient with ID '{patientId}' was not found.")
    {
    }
}
