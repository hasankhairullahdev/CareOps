using PharmacyService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace PharmacyService.Application.Pharmacy.Queries;

public record SupplierDto(
    Guid Id, string Name, string? ContactPerson,
    string? Phone, string? Email, string? Address,
    bool IsActive, DateTime CreatedAt);

public record GetSuppliersQuery(bool? IsActive = null, int Page = 1, int PageSize = 50)
    : IRequest<GetSuppliersResult>;

public record GetSuppliersResult(IReadOnlyList<SupplierDto> Items, int TotalCount);

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, GetSuppliersResult>
{
    private readonly PharmacyDbContext _context;
    public GetSuppliersQueryHandler(PharmacyDbContext context) => _context = context;

    public async Task<GetSuppliersResult> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Suppliers.AsNoTracking().AsQueryable();
        if (request.IsActive.HasValue)
            query = query.Where(s => s.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SupplierDto(s.Id, s.Name, s.ContactPerson, s.Phone, s.Email, s.Address, s.IsActive, s.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetSuppliersResult(items, total);
    }
}
