namespace AppointmentService.Domain.Exceptions;

public sealed class AppointmentCannotBeCancelledException : Exception
{
    public AppointmentCannotBeCancelledException(Guid appointmentId, string currentStatus)
        : base($"Appointment '{appointmentId}' cannot be cancelled. Current status: {currentStatus}.")
    {
    }
}
