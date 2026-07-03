using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyService.Domain.Enums;
using PharmacyService.Domain.Exceptions;
using PharmacyService.Infrastructure.Persistence;

namespace PharmacyService.Application.Pharmacy.Queries;

public record GetPrescriptionByIdQuery(Guid Id) : IRequest<PrescriptionDto>;

public record PrescriptionDto(
    Guid Id,
    Guid ExternalPrescriptionId,
    Guid PatientId,
    Guid AppointmentId,
    PrescriptionStatus Status,
    DateTime CreatedAt,
    DateTime? DispensedAt,
    IReadOnlyList<PrescriptionItemDto> Items);

public record PrescriptionItemDto(
    Guid Id,
    string MedicineName,
    int Quantity,
    string Dosage,
    string Instructions);

public record GetPendingPrescriptionsQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedPrescriptionsResult>;
public record PaginatedPrescriptionsResult(IReadOnlyList<PrescriptionDto> Items, int TotalCount);

public class GetPrescriptionByIdQueryHandler : IRequestHandler<GetPrescriptionByIdQuery, PrescriptionDto>
{
    private readonly PharmacyDbContext _context;
    public GetPrescriptionByIdQueryHandler(PharmacyDbContext context) => _context = context;

    public async Task<PrescriptionDto> Handle(GetPrescriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.Prescriptions
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new PrescriptionNotFoundException(request.Id);

        return ToDto(p);
    }

    internal static PrescriptionDto ToDto(Domain.Entities.Prescription p) =>
        new(p.Id, p.ExternalPrescriptionId, p.PatientId, p.AppointmentId, p.Status,
            p.CreatedAt, p.DispensedAt,
            p.Items.Select(i => new PrescriptionItemDto(i.Id, i.MedicineName, i.Quantity, i.Dosage, i.Instructions)).ToList());
}

public class GetPendingPrescriptionsQueryHandler : IRequestHandler<GetPendingPrescriptionsQuery, PaginatedPrescriptionsResult>
{
    private readonly PharmacyDbContext _context;
    public GetPendingPrescriptionsQueryHandler(PharmacyDbContext context) => _context = context;

    public async Task<PaginatedPrescriptionsResult> Handle(GetPendingPrescriptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Prescriptions
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.Status == PrescriptionStatus.Pending);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedPrescriptionsResult(
            items.Select(GetPrescriptionByIdQueryHandler.ToDto).ToList(),
            total);
    }
}
