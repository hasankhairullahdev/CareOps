using BillingService.Domain.Entities;
using BillingService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Application.Billing.Consumers;

// ── Event contracts (mirror publishers) ──────────────────────────────────────

public record AppointmentCreatedMessage(
    Guid AppointmentId, Guid PatientId, Guid DoctorId,
    string DoctorName, DateTime ScheduledAt, DateTime CreatedAt);

public record AppointmentCancelledMessage(
    Guid AppointmentId, Guid PatientId, string? Reason);

public record PrescriptionDispensedMessage(
    Guid PrescriptionId, Guid ExternalPrescriptionId,
    Guid PatientId, Guid AppointmentId,
    List<DispensedItemMessage> Items, decimal TotalAmount, DateTime DispensedAt);

public record DispensedItemMessage(
    string MedicineName, int Quantity, decimal UnitPrice, decimal TotalPrice);

// ── Consumers ─────────────────────────────────────────────────────────────────

/// <summary>
/// Idempotent: check AppointmentId unique index before creating bill.
/// </summary>
public class AppointmentCreatedConsumer : IConsumer<AppointmentCreatedMessage>
{
    private readonly BillingDbContext _context;
    public AppointmentCreatedConsumer(BillingDbContext context) => _context = context;

    public async Task Consume(ConsumeContext<AppointmentCreatedMessage> context)
    {
        var msg = context.Message;

        // Idempotency check
        var exists = await _context.Bills
            .AnyAsync(b => b.AppointmentId == msg.AppointmentId, context.CancellationToken);
        if (exists) return;

        var bill = Bill.Create(
            msg.PatientId,
            msg.AppointmentId,
            $"Biaya konsultasi dengan {msg.DoctorName}",
            150_000m); // default consultation fee

        _context.Bills.Add(bill);
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}

/// <summary>
/// Adds medicine line items to existing bill. Idempotent: checks if already added.
/// </summary>
public class PrescriptionDispensedConsumer : IConsumer<PrescriptionDispensedMessage>
{
    private readonly BillingDbContext _context;
    public PrescriptionDispensedConsumer(BillingDbContext context) => _context = context;

    public async Task Consume(ConsumeContext<PrescriptionDispensedMessage> context)
    {
        var msg = context.Message;

        var bill = await _context.Bills
            .Include(b => b.LineItems)
            .FirstOrDefaultAsync(b => b.AppointmentId == msg.AppointmentId, context.CancellationToken);
        if (bill is null) return;

        // Idempotency: skip if prescription line items already added
        var prescriptionDesc = $"Obat resep #{msg.PrescriptionId}";
        var alreadyAdded = bill.LineItems.Any(l => l.Description.Contains(msg.PrescriptionId.ToString()));
        if (alreadyAdded) return;

        foreach (var item in msg.Items.Where(i => i.UnitPrice > 0))
            bill.AddLineItem($"Obat: {item.MedicineName}", item.Quantity, item.UnitPrice);

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}

/// <summary>
/// Cancels bill when appointment is cancelled.
/// </summary>
public class AppointmentCancelledConsumer : IConsumer<AppointmentCancelledMessage>
{
    private readonly BillingDbContext _context;
    public AppointmentCancelledConsumer(BillingDbContext context) => _context = context;

    public async Task Consume(ConsumeContext<AppointmentCancelledMessage> context)
    {
        var msg = context.Message;

        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.AppointmentId == msg.AppointmentId, context.CancellationToken);
        if (bill is null || bill.Status == Domain.Enums.BillStatus.Cancelled) return;

        bill.Cancel();
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}
