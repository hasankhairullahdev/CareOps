using BillingService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Application.Billing.Queries;

public record PaymentMethodDto(Guid Id, string Name, string? Description, bool IsActive, DateTime CreatedAt);

public record GetPaymentMethodsQuery(bool? IsActive = null) : IRequest<IReadOnlyList<PaymentMethodDto>>;

public class GetPaymentMethodsQueryHandler : IRequestHandler<GetPaymentMethodsQuery, IReadOnlyList<PaymentMethodDto>>
{
    private readonly BillingDbContext _context;
    public GetPaymentMethodsQueryHandler(BillingDbContext context) => _context = context;

    public async Task<IReadOnlyList<PaymentMethodDto>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PaymentMethods.AsNoTracking().AsQueryable();
        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new PaymentMethodDto(p.Id, p.Name, p.Description, p.IsActive, p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
