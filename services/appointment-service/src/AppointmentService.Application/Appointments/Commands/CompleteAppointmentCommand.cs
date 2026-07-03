using AppointmentService.Domain.Exceptions;
using AppointmentService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.Appointments.Commands;

public record CompleteAppointmentCommand(Guid Id) : IRequest;

public class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand>
{
    private readonly AppointmentDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CompleteAppointmentCommandHandler(AppointmentDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new AppointmentNotFoundException(request.Id);

        appointment.Complete();
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new AppointmentCompletedEvent(
            appointment.Id,
            appointment.PatientId,
            appointment.DoctorId), cancellationToken);
    }
}

public record AppointmentCompletedEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId);
