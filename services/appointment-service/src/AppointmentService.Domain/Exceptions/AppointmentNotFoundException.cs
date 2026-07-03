namespace AppointmentService.Domain.Exceptions;

public sealed class AppointmentNotFoundException : Exception
{
    public AppointmentNotFoundException(Guid appointmentId)
        : base($"Appointment with ID '{appointmentId}' was not found.")
    {
    }
}
