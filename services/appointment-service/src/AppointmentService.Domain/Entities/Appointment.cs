using AppointmentService.Domain.Enums;
using AppointmentService.Domain.Exceptions;

namespace AppointmentService.Domain.Entities;

public class Appointment
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public Doctor? Doctor { get; private set; }

    private Appointment() { }

    public static Appointment Create(Guid patientId, Guid doctorId, DateTime scheduledAt, string? notes = null)
    {
        return new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            ScheduledAt = scheduledAt,
            Status = AppointmentStatus.Scheduled,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Cancel(string? reason = null)
    {
        if (Status == AppointmentStatus.Completed)
            throw new AppointmentCannotBeCancelledException(Id, Status.ToString());

        Status = AppointmentStatus.Cancelled;
        Notes = reason ?? Notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.Scheduled && Status != AppointmentStatus.InProgress)
            throw new AppointmentCannotBeCompletedException(Id, Status.ToString());

        Status = AppointmentStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartProgress()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new AppointmentCannotBeCompletedException(Id, Status.ToString());

        Status = AppointmentStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }
}
