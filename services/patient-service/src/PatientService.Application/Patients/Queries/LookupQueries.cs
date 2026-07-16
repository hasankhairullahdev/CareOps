using PatientService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace PatientService.Application.Patients.Queries;

// ── BloodType Queries ─────────────────────────────────────────────────────────

public record BloodTypeDto(int Id, string Name);
public record GetBloodTypesQuery : IRequest<IReadOnlyList<BloodTypeDto>>;

public class GetBloodTypesQueryHandler : IRequestHandler<GetBloodTypesQuery, IReadOnlyList<BloodTypeDto>>
{
    private readonly PatientDbContext _context;
    public GetBloodTypesQueryHandler(PatientDbContext context) => _context = context;

    public async Task<IReadOnlyList<BloodTypeDto>> Handle(GetBloodTypesQuery request, CancellationToken cancellationToken)
    {
        return await _context.BloodTypes
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => new BloodTypeDto(b.Id, b.Name))
            .ToListAsync(cancellationToken);
    }
}

// ── AllergyType Queries ───────────────────────────────────────────────────────

public record AllergyTypeDto(Guid Id, string Name, string? Description, bool IsActive, DateTime CreatedAt);
public record GetAllergyTypesQuery(bool? IsActive = null) : IRequest<IReadOnlyList<AllergyTypeDto>>;

public class GetAllergyTypesQueryHandler : IRequestHandler<GetAllergyTypesQuery, IReadOnlyList<AllergyTypeDto>>
{
    private readonly PatientDbContext _context;
    public GetAllergyTypesQueryHandler(PatientDbContext context) => _context = context;

    public async Task<IReadOnlyList<AllergyTypeDto>> Handle(GetAllergyTypesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AllergyTypes.AsNoTracking().AsQueryable();
        if (request.IsActive.HasValue)
            query = query.Where(a => a.IsActive == request.IsActive.Value);

        return await query
            .OrderBy(a => a.Name)
            .Select(a => new AllergyTypeDto(a.Id, a.Name, a.Description, a.IsActive, a.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
