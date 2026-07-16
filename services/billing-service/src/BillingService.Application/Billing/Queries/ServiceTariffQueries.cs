using BillingService.Domain.Exceptions;
using BillingService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Application.Billing.Queries;

// ── ServiceTariff DTOs ─────────────────────────────────────────────────────────

public record ServiceTariffDto(
    Guid Id, string ServiceName, string Category,
    decimal Price, string? Description, bool IsActive,
    DateTime CreatedAt, DateTime? UpdatedAt);

// ── GetServiceTariffs ─────────────────────────────────────────────────────────

public record GetServiceTariffsQuery(
    string? Search = null,
    string? Category = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 50) : IRequest<PaginatedTariffsResult>;

public record PaginatedTariffsResult(IReadOnlyList<ServiceTariffDto> Items, int TotalCount);

public class GetServiceTariffsQueryHandler : IRequestHandler<GetServiceTariffsQuery, PaginatedTariffsResult>
{
    private readonly BillingDbContext _context;
    public GetServiceTariffsQueryHandler(BillingDbContext context) => _context = context;

    public async Task<PaginatedTariffsResult> Handle(GetServiceTariffsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ServiceTariffs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => t.ServiceName.Contains(request.Search) || t.Category.Contains(request.Search));

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(t => t.Category == request.Category);

        if (request.IsActive.HasValue)
            query = query.Where(t => t.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(t => t.Category)
            .ThenBy(t => t.ServiceName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new ServiceTariffDto(
                t.Id, t.ServiceName, t.Category, t.Price,
                t.Description, t.IsActive, t.CreatedAt, t.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedTariffsResult(items, total);
    }
}

// ── GetServiceTariffById ──────────────────────────────────────────────────────

public record GetServiceTariffByIdQuery(Guid Id) : IRequest<ServiceTariffDto>;

public class GetServiceTariffByIdQueryHandler : IRequestHandler<GetServiceTariffByIdQuery, ServiceTariffDto>
{
    private readonly BillingDbContext _context;
    public GetServiceTariffByIdQueryHandler(BillingDbContext context) => _context = context;

    public async Task<ServiceTariffDto> Handle(GetServiceTariffByIdQuery request, CancellationToken cancellationToken)
    {
        var t = await _context.ServiceTariffs
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new ServiceTariffNotFoundException(request.Id);

        return new ServiceTariffDto(
            t.Id, t.ServiceName, t.Category, t.Price,
            t.Description, t.IsActive, t.CreatedAt, t.UpdatedAt);
    }
}
