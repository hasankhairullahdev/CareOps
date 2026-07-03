namespace AppointmentService.Domain.Exceptions;

public sealed class AppointmentConflictException : Exception
{
    public AppointmentConflictException(Guid doctorId, DateTime scheduledAt)
        : base($"Doctor '{doctorId}' already has an appointment at '{scheduledAt:yyyy-MM-dd HH:mm}'.")
    {
    }
}
