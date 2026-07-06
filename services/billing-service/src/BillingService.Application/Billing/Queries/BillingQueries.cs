using BillingService.Domain.Enums;
using BillingService.Domain.Exceptions;
using BillingService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Application.Billing.Queries;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record BillDto(
    Guid Id, Guid PatientId, Guid AppointmentId,
    BillStatus Status, decimal TotalAmount,
    DateTime CreatedAt, DateTime? IssuedAt, DateTime? PaidAt,
    IReadOnlyList<BillLineItemDto> LineItems);

public record BillLineItemDto(
    Guid Id, string Description, int Quantity, decimal UnitPrice, decimal Amount);

// ── GetBillById ───────────────────────────────────────────────────────────────

public record GetBillByIdQuery(Guid Id) : IRequest<BillDto>;

public class GetBillByIdQueryHandler : IRequestHandler<GetBillByIdQuery, BillDto>
{
    private readonly BillingDbContext _context;
    public GetBillByIdQueryHandler(BillingDbContext context) => _context = context;

    public async Task<BillDto> Handle(GetBillByIdQuery request, CancellationToken cancellationToken)
    {
        var bill = await _context.Bills
            .AsNoTracking()
            .Include(b => b.LineItems)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new BillNotFoundException(request.Id);

        return ToDto(bill);
    }

    internal static BillDto ToDto(Domain.Entities.Bill b) =>
        new(b.Id, b.PatientId, b.AppointmentId, b.Status, b.TotalAmount,
            b.CreatedAt, b.IssuedAt, b.PaidAt,
            b.LineItems.Select(l => new BillLineItemDto(l.Id, l.Description, l.Quantity, l.UnitPrice, l.Amount)).ToList());
}

// ── GetBillsByPatient ─────────────────────────────────────────────────────────

public record GetBillsByPatientQuery(Guid PatientId, int Page = 1, int PageSize = 20)
    : IRequest<PaginatedBillsResult>;

public record PaginatedBillsResult(IReadOnlyList<BillDto> Items, int TotalCount);

public class GetBillsByPatientQueryHandler : IRequestHandler<GetBillsByPatientQuery, PaginatedBillsResult>
{
    private readonly BillingDbContext _context;
    public GetBillsByPatientQueryHandler(BillingDbContext context) => _context = context;

    public async Task<PaginatedBillsResult> Handle(GetBillsByPatientQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Bills.AsNoTracking().Include(b => b.LineItems)
            .Where(b => b.PatientId == request.PatientId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedBillsResult(items.Select(GetBillByIdQueryHandler.ToDto).ToList(), total);
    }
}

// ── GetBillsSummary (cashier dashboard) ───────────────────────────────────────

public record GetBillsSummaryQuery : IRequest<BillsSummaryDto>;

public record BillsSummaryDto(
    int PendingCount, decimal PendingAmount,
    int PaidTodayCount, decimal PaidTodayAmount,
    int TotalTodayCount);

public class GetBillsSummaryQueryHandler : IRequestHandler<GetBillsSummaryQuery, BillsSummaryDto>
{
    private readonly BillingDbContext _context;
    public GetBillsSummaryQueryHandler(BillingDbContext context) => _context = context;

    public async Task<BillsSummaryDto> Handle(GetBillsSummaryQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var pending = await _context.Bills
            .Where(b => b.Status == BillStatus.Issued)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(b => b.TotalAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        var paidToday = await _context.Bills
            .Where(b => b.Status == BillStatus.Paid && b.PaidAt!.Value.Date == today)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(b => b.TotalAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        var totalToday = await _context.Bills
            .CountAsync(b => b.CreatedAt.Date == today, cancellationToken);

        return new BillsSummaryDto(
            pending?.Count ?? 0, pending?.Amount ?? 0m,
            paidToday?.Count ?? 0, paidToday?.Amount ?? 0m,
            totalToday);
    }
}
