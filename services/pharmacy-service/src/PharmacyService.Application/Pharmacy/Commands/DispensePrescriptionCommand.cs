using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyService.Domain.Enums;
using PharmacyService.Domain.Exceptions;
using PharmacyService.Infrastructure.Persistence;

namespace PharmacyService.Application.Pharmacy.Commands;

public record DispensePrescriptionCommand(Guid PrescriptionId) : IRequest<DispensePrescriptionResult>;

public record DispensePrescriptionResult(Guid PrescriptionId, List<DispensedItemResult> Items);
public record DispensedItemResult(string MedicineName, int Quantity, decimal UnitPrice, decimal TotalPrice);

public class DispensePrescriptionCommandHandler : IRequestHandler<DispensePrescriptionCommand, DispensePrescriptionResult>
{
    private readonly PharmacyDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public DispensePrescriptionCommandHandler(PharmacyDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<DispensePrescriptionResult> Handle(DispensePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId, cancellationToken)
            ?? throw new PrescriptionNotFoundException(request.PrescriptionId);

        if (prescription.Status == PrescriptionStatus.Dispensed)
            throw new PrescriptionAlreadyDispensedException(prescription.Id);

        var dispensedItems = new List<DispensedItemResult>();

        foreach (var item in prescription.Items)
        {
            // Try to find matching medicine by name (case-insensitive)
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.Name.ToLower() == item.MedicineName.ToLower(), cancellationToken);

            if (medicine is not null)
            {
                medicine.DeductStock(item.Quantity);

                var movement = Domain.Entities.StockMovement.Create(
                    medicine.Id, StockMovementType.Out, item.Quantity,
                    $"Dispensed for prescription {prescription.Id}");
                _context.StockMovements.Add(movement);

                dispensedItems.Add(new DispensedItemResult(
                    item.MedicineName, item.Quantity,
                    medicine.Price, medicine.Price * item.Quantity));
            }
            else
            {
                dispensedItems.Add(new DispensedItemResult(item.MedicineName, item.Quantity, 0m, 0m));
            }
        }

        prescription.Dispense();
        await _context.SaveChangesAsync(cancellationToken);

        var totalAmount = dispensedItems.Sum(i => i.TotalPrice);

        await _publishEndpoint.Publish(new PrescriptionDispensedEvent(
            prescription.Id,
            prescription.ExternalPrescriptionId,
            prescription.PatientId,
            prescription.AppointmentId,
            dispensedItems.Select(i => new DispensedItemEvent(i.MedicineName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList(),
            totalAmount,
            prescription.DispensedAt!.Value), cancellationToken);

        return new DispensePrescriptionResult(prescription.Id, dispensedItems);
    }
}

public record PrescriptionDispensedEvent(
    Guid PrescriptionId,
    Guid ExternalPrescriptionId,
    Guid PatientId,
    Guid AppointmentId,
    List<DispensedItemEvent> Items,
    decimal TotalAmount,
    DateTime DispensedAt);

public record DispensedItemEvent(
    string MedicineName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
