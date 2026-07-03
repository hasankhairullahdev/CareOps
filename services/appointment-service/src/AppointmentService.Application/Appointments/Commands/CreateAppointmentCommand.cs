using AppointmentService.Domain.Entities;
using AppointmentService.Domain.Exceptions;
using AppointmentService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.Appointments.Commands;

public record CreateAppointmentCommand(
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAt,
    string? Notes) : IRequest<CreateAppointmentResult>;

public record CreateAppointmentResult(Guid Id, DateTime ScheduledAt);

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, CreateAppointmentResult>
{
    private readonly AppointmentDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateAppointmentCommandHandler(AppointmentDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateAppointmentResult> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        // Check doctor exists
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken)
            ?? throw new DoctorNotAvailableException(request.DoctorId, request.ScheduledAt);

        // Check for conflict (doctor can't have two appointments at same time ± 30 min)
        var windowStart = request.ScheduledAt.AddMinutes(-30);
        var windowEnd = request.ScheduledAt.AddMinutes(30);
        var conflict = await _context.Appointments
            .AnyAsync(a =>
                a.DoctorId == request.DoctorId &&
                a.Status != AppointmentService.Domain.Enums.AppointmentStatus.Cancelled &&
                a.ScheduledAt >= windowStart &&
                a.ScheduledAt <= windowEnd,
                cancellationToken);

        if (conflict)
            throw new AppointmentConflictException(request.DoctorId, request.ScheduledAt);

        var appointment = Appointment.Create(
            request.PatientId,
            request.DoctorId,
            request.ScheduledAt,
            request.Notes);

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new AppointmentCreatedEvent(
            appointment.Id,
            appointment.PatientId,
            appointment.DoctorId,
            doctor.Name,
            appointment.ScheduledAt,
            appointment.CreatedAt), cancellationToken);

        return new CreateAppointmentResult(appointment.Id, appointment.ScheduledAt);
    }
}

public record AppointmentCreatedEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    string DoctorName,
    DateTime ScheduledAt,
    DateTime CreatedAt);
