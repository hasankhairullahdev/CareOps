namespace AppointmentService.Domain.Exceptions;

public sealed class DoctorNotAvailableException : Exception
{
    public DoctorNotAvailableException(Guid doctorId, DateTime scheduledAt)
        : base($"Doctor '{doctorId}' is not available at '{scheduledAt:yyyy-MM-dd HH:mm}'.")
    {
    }
}
