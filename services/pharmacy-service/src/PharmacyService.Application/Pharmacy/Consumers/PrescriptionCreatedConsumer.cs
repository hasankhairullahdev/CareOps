using MassTransit;
using Microsoft.EntityFrameworkCore;
using PharmacyService.Domain.Entities;
using PharmacyService.Infrastructure.Persistence;

namespace PharmacyService.Application.Pharmacy.Consumers;

/// <summary>
/// Consumes PrescriptionCreated event from appointment-service.
/// Idempotent: checks ExternalPrescriptionId before creating.
/// </summary>
public class PrescriptionCreatedConsumer : IConsumer<PrescriptionCreatedMessage>
{
    private readonly PharmacyDbContext _context;

    public PrescriptionCreatedConsumer(PharmacyDbContext context) => _context = context;

    public async Task Consume(ConsumeContext<PrescriptionCreatedMessage> context)
    {
        var msg = context.Message;

        // Idempotency: skip if already processed
        var exists = await _context.Prescriptions
            .AnyAsync(p => p.ExternalPrescriptionId == msg.PrescriptionId, context.CancellationToken);
        if (exists) return;

        var items = msg.Items.Select(i =>
            PrescriptionItem.Create(Guid.Empty, i.MedicineName, i.Quantity, i.Dosage, i.Instructions)
        ).ToList();

        var prescription = Prescription.CreateFromEvent(
            msg.PrescriptionId,
            msg.PatientId,
            msg.AppointmentId,
            items);

        // Fix PrescriptionId FK on items
        foreach (var item in prescription.Items)
        {
            var field = typeof(PrescriptionItem).GetField("<PrescriptionId>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(item, prescription.Id);
        }

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}

// Mirror of the event published by appointment-service
public record PrescriptionCreatedMessage(
    Guid PrescriptionId,
    Guid AppointmentId,
    Guid PatientId,
    List<PrescriptionItemMessage> Items,
    DateTime CreatedAt);

public record PrescriptionItemMessage(
    string MedicineName,
    int Quantity,
    string Dosage,
    string Instructions);
