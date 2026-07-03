using AppointmentService.Domain.Entities;
using AppointmentService.Domain.Exceptions;
using AppointmentService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.Appointments.Commands;

public record CreatePrescriptionCommand(
    Guid AppointmentId,
    List<PrescriptionItemRequest> Items) : IRequest<CreatePrescriptionResult>;

public record PrescriptionItemRequest(
    string MedicineName,
    int Quantity,
    string Dosage,
    string Instructions);

public record CreatePrescriptionResult(Guid PrescriptionId);

public class CreatePrescriptionCommandHandler : IRequestHandler<CreatePrescriptionCommand, CreatePrescriptionResult>
{
    private readonly AppointmentDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreatePrescriptionCommandHandler(AppointmentDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreatePrescriptionResult> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken)
            ?? throw new AppointmentNotFoundException(request.AppointmentId);

        var items = request.Items
            .Select(i => PrescriptionItem.Create(Guid.Empty, i.MedicineName, i.Quantity, i.Dosage, i.Instructions))
            .ToList();

        var prescription = Prescription.Create(appointment.Id, appointment.PatientId, items);

        // Fix prescription ID references
        foreach (var item in prescription.Items)
        {
            var field = typeof(PrescriptionItem).GetField("<PrescriptionId>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(item, prescription.Id);
        }

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new PrescriptionCreatedEvent(
            prescription.Id,
            prescription.AppointmentId,
            prescription.PatientId,
            prescription.Items.Select(i => new PrescriptionItemEvent(
                i.MedicineName, i.Quantity, i.Dosage, i.Instructions)).ToList(),
            prescription.CreatedAt), cancellationToken);

        return new CreatePrescriptionResult(prescription.Id);
    }
}

public record PrescriptionCreatedEvent(
    Guid PrescriptionId,
    Guid AppointmentId,
    Guid PatientId,
    List<PrescriptionItemEvent> Items,
    DateTime CreatedAt);

public record PrescriptionItemEvent(
    string MedicineName,
    int Quantity,
    string Dosage,
    string Instructions);
