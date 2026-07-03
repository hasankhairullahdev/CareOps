using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyService.Infrastructure.Persistence;

namespace PharmacyService.Application.Pharmacy.Queries;

public record GetInventoryQuery(
    bool? LowStockOnly = null,
    bool? ExpiringSoonOnly = null,
    int Page = 1,
    int PageSize = 50) : IRequest<InventoryResult>;

public record InventoryResult(IReadOnlyList<MedicineDto> Items, int TotalCount, int LowStockCount);

public record MedicineDto(
    Guid Id,
    string Name,
    string GenericName,
    string Category,
    string Unit,
    int StockQuantity,
    int MinimumStock,
    decimal Price,
    DateOnly ExpiryDate,
    bool IsLowStock,
    bool IsExpiringSoon);

public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, InventoryResult>
{
    private readonly PharmacyDbContext _context;

    public GetInventoryQueryHandler(PharmacyDbContext context) => _context = context;

    public async Task<InventoryResult> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var soonThreshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var query = _context.Medicines.AsNoTracking();

        if (request.LowStockOnly == true)
            query = query.Where(m => m.StockQuantity <= m.MinimumStock);

        if (request.ExpiringSoonOnly == true)
            query = query.Where(m => m.ExpiryDate <= soonThreshold);

        var totalCount = await query.CountAsync(cancellationToken);
        var lowStockCount = await _context.Medicines.CountAsync(m => m.StockQuantity <= m.MinimumStock, cancellationToken);

        var medicines = await query
            .OrderBy(m => m.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MedicineDto(
                m.Id, m.Name, m.GenericName, m.Category, m.Unit,
                m.StockQuantity, m.MinimumStock, m.Price, m.ExpiryDate,
                m.StockQuantity <= m.MinimumStock,
                m.ExpiryDate <= soonThreshold))
            .ToListAsync(cancellationToken);

        return new InventoryResult(medicines, totalCount, lowStockCount);
    }
}
