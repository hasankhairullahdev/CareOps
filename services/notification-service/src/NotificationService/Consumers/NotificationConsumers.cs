using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Storage;

namespace NotificationService.Consumers;

// ── Shared event message types (mirror publisher contracts) ──────────────────

public record PatientRegisteredMessage(Guid PatientId, string FirstName, string LastName, string Email, string MedicalRecordNumber, DateTime RegisteredAt);
public record AppointmentCreatedMessage(Guid AppointmentId, Guid PatientId, Guid DoctorId, string DoctorName, DateTime ScheduledAt, DateTime CreatedAt);
public record AppointmentCancelledMessage(Guid AppointmentId, Guid PatientId, string? Reason);
public record PrescriptionDispensedMessage(Guid PrescriptionId, Guid ExternalPrescriptionId, Guid PatientId, Guid AppointmentId, List<object> Items, decimal TotalAmount, DateTime DispensedAt);
public record BillGeneratedMessage(Guid BillId, Guid PatientId, Guid AppointmentId, decimal TotalAmount, DateTime IssuedAt);
public record BillPaidMessage(Guid BillId, Guid PatientId, decimal AmountPaid, DateTime PaidAt);

// ── Consumers ────────────────────────────────────────────────────────────────

public class PatientRegisteredConsumer : IConsumer<PatientRegisteredMessage>
{
    private readonly INotificationStore _store;
    private readonly ILogger<PatientRegisteredConsumer> _logger;
    public PatientRegisteredConsumer(INotificationStore store, ILogger<PatientRegisteredConsumer> logger) { _store = store; _logger = logger; }

    public Task Consume(ConsumeContext<PatientRegisteredMessage> context)
    {
        var msg = context.Message;
        var message = $"Selamat datang, {msg.FirstName} {msg.LastName}! No. Rekam Medis Anda: {msg.MedicalRecordNumber}.";
        _store.Add(msg.PatientId.ToString(), "PatientRegistered", message);
        _logger.LogInformation("[Notification] PatientRegistered: {Email}", msg.Email);
        return Task.CompletedTask;
    }
}

public class AppointmentCreatedConsumer : IConsumer<AppointmentCreatedMessage>
{
    private readonly INotificationStore _store;
    private readonly ILogger<AppointmentCreatedConsumer> _logger;
    public AppointmentCreatedConsumer(INotificationStore store, ILogger<AppointmentCreatedConsumer> logger) { _store = store; _logger = logger; }

    public Task Consume(ConsumeContext<AppointmentCreatedMessage> context)
    {
        var msg = context.Message;
        var message = $"Appointment Anda dengan {msg.DoctorName} dijadwalkan pada {msg.ScheduledAt:dd MMM yyyy HH:mm}. ID: {msg.AppointmentId}.";
        _store.Add(msg.PatientId.ToString(), "AppointmentCreated", message);
        _logger.LogInformation("[Notification] AppointmentCreated: PatientId={PatientId}", msg.PatientId);
        return Task.CompletedTask;
    }
}

public class AppointmentCancelledConsumer : IConsumer<AppointmentCancelledMessage>
{
    private readonly INotificationStore _store;
    private readonly ILogger<AppointmentCancelledConsumer> _logger;
    public AppointmentCancelledConsumer(INotificationStore store, ILogger<AppointmentCancelledConsumer> logger) { _store = store; _logger = logger; }

    public Task Consume(ConsumeContext<AppointmentCancelledMessage> context)
    {
        var msg = context.Message;
        var reason = string.IsNullOrEmpty(msg.Reason) ? "" : $" Alasan: {msg.Reason}.";
        var message = $"Appointment {msg.AppointmentId} telah dibatalkan.{reason}";
        _store.Add(msg.PatientId.ToString(), "AppointmentCancelled", message);
        _logger.LogInformation("[Notification] AppointmentCancelled: PatientId={PatientId}", msg.PatientId);
        return Task.CompletedTask;
    }
}

public class PrescriptionDispensedConsumer : IConsumer<PrescriptionDispensedMessage>
{
    private readonly INotificationStore _store;
    private readonly ILogger<PrescriptionDispensedConsumer> _logger;
    public PrescriptionDispensedConsumer(INotificationStore store, ILogger<PrescriptionDispensedConsumer> logger) { _store = store; _logger = logger; }

    public Task Consume(ConsumeContext<PrescriptionDispensedMessage> context)
    {
        var msg = context.Message;
        var message = $"Obat Anda sudah siap diambil. Total: Rp {msg.TotalAmount:N0}.";
        _store.Add(msg.PatientId.ToString(), "PrescriptionDispensed", message);
        _logger.LogInformation("[Notification] PrescriptionDispensed: PatientId={PatientId}", msg.PatientId);
        return Task.CompletedTask;
    }
}

public class BillGeneratedConsumer : IConsumer<BillGeneratedMessage>
{
    private readonly INotificationStore _store;
    private readonly ILogger<BillGeneratedConsumer> _logger;
    public BillGeneratedConsumer(INotificationStore store, ILogger<BillGeneratedConsumer> logger) { _store = store; _logger = logger; }

    public Task Consume(ConsumeContext<BillGeneratedMessage> context)
    {
        var msg = context.Message;
        var message = $"Tagihan Anda telah diterbitkan. Total: Rp {msg.TotalAmount:N0}. ID Tagihan: {msg.BillId}.";
        _store.Add(msg.PatientId.ToString(), "BillGenerated", message);
        _logger.LogInformation("[Notification] BillGenerated: PatientId={PatientId}", msg.PatientId);
        return Task.CompletedTask;
    }
}

public class BillPaidConsumer : IConsumer<BillPaidMessage>
{
    private readonly INotificationStore _store;
    private readonly ILogger<BillPaidConsumer> _logger;
    public BillPaidConsumer(INotificationStore store, ILogger<BillPaidConsumer> logger) { _store = store; _logger = logger; }

    public Task Consume(ConsumeContext<BillPaidMessage> context)
    {
        var msg = context.Message;
        var message = $"Pembayaran tagihan {msg.BillId} sebesar Rp {msg.AmountPaid:N0} berhasil. Terima kasih!";
        _store.Add(msg.PatientId.ToString(), "BillPaid", message);
        _logger.LogInformation("[Notification] BillPaid: PatientId={PatientId}", msg.PatientId);
        return Task.CompletedTask;
    }
}
