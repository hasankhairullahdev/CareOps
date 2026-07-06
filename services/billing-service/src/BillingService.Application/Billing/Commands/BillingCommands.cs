using BillingService.Domain.Exceptions;
using BillingService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Application.Billing.Commands;

// ── Issue Bill ────────────────────────────────────────────────────────────────

public record IssueBillCommand(Guid BillId) : IRequest;

public class IssueBillCommandHandler : IRequestHandler<IssueBillCommand>
{
    private readonly BillingDbContext _context;
    private readonly IPublishEndpoint _publish;
    public IssueBillCommandHandler(BillingDbContext context, IPublishEndpoint publish)
    {
        _context = context; _publish = publish;
    }

    public async Task Handle(IssueBillCommand request, CancellationToken cancellationToken)
    {
        var bill = await _context.Bills.Include(b => b.LineItems)
            .FirstOrDefaultAsync(b => b.Id == request.BillId, cancellationToken)
            ?? throw new BillNotFoundException(request.BillId);

        bill.Issue();
        await _context.SaveChangesAsync(cancellationToken);

        await _publish.Publish(new BillGeneratedEvent(
            bill.Id, bill.PatientId, bill.AppointmentId,
            bill.TotalAmount, bill.IssuedAt!.Value), cancellationToken);
    }
}

public record BillGeneratedEvent(
    Guid BillId, Guid PatientId, Guid AppointmentId,
    decimal TotalAmount, DateTime IssuedAt);

// ── Process Payment ───────────────────────────────────────────────────────────

public record ProcessPaymentCommand(Guid BillId) : IRequest<ProcessPaymentResult>;
public record ProcessPaymentResult(Guid BillId, decimal AmountPaid, DateTime PaidAt);

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    private readonly BillingDbContext _context;
    private readonly IPublishEndpoint _publish;
    public ProcessPaymentCommandHandler(BillingDbContext context, IPublishEndpoint publish)
    {
        _context = context; _publish = publish;
    }

    public async Task<ProcessPaymentResult> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == request.BillId, cancellationToken)
            ?? throw new BillNotFoundException(request.BillId);

        bill.Pay();
        await _context.SaveChangesAsync(cancellationToken);

        await _publish.Publish(new BillPaidEvent(
            bill.Id, bill.PatientId, bill.TotalAmount, bill.PaidAt!.Value), cancellationToken);

        return new ProcessPaymentResult(bill.Id, bill.TotalAmount, bill.PaidAt!.Value);
    }
}

public record BillPaidEvent(
    Guid BillId, Guid PatientId, decimal AmountPaid, DateTime PaidAt);

// ── Cancel Bill ───────────────────────────────────────────────────────────────

public record CancelBillCommand(Guid BillId) : IRequest;

public class CancelBillCommandHandler : IRequestHandler<CancelBillCommand>
{
    private readonly BillingDbContext _context;
    public CancelBillCommandHandler(BillingDbContext context) => _context = context;

    public async Task Handle(CancelBillCommand request, CancellationToken cancellationToken)
    {
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == request.BillId, cancellationToken)
            ?? throw new BillNotFoundException(request.BillId);

        bill.Cancel();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
