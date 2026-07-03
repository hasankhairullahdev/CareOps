using AppointmentService.Domain.Exceptions;
using AppointmentService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.Appointments.Commands;

public record CancelAppointmentCommand(Guid Id, string? Reason) : IRequest;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand>
{
    private readonly AppointmentDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CancelAppointmentCommandHandler(AppointmentDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new AppointmentNotFoundException(request.Id);

        appointment.Cancel(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new AppointmentCancelledEvent(
            appointment.Id,
            appointment.PatientId,
            request.Reason), cancellationToken);
    }
}

public record AppointmentCancelledEvent(
    Guid AppointmentId,
    Guid PatientId,
    string? Reason);
