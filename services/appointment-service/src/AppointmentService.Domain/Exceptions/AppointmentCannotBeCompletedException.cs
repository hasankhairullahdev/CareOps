namespace AppointmentService.Domain.Exceptions;

public sealed class AppointmentCannotBeCompletedException : Exception
{
    public AppointmentCannotBeCompletedException(Guid appointmentId, string currentStatus)
        : base($"Appointment '{appointmentId}' cannot be completed. Current status: {currentStatus}.")
    {
    }
}
