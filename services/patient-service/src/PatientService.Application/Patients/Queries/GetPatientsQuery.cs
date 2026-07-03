using PatientService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace PatientService.Application.Patients.Queries;

public record GetPatientsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<PaginatedPatientsResult>;

public record PaginatedPatientsResult(
    IReadOnlyList<PatientDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, PaginatedPatientsResult>
{
    private readonly PatientDbContext _context;

    public GetPatientsQueryHandler(PatientDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedPatientsResult> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(search) ||
                p.LastName.ToLower().Contains(search) ||
                p.Email.ToLower().Contains(search) ||
                p.MedicalRecordNumber.ToLower().Contains(search) ||
                p.PhoneNumber.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var patients = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PatientDto(
                p.Id,
                p.FirstName,
                p.LastName,
                p.DateOfBirth,
                p.Gender,
                p.PhoneNumber,
                p.Email,
                p.Address,
                p.MedicalRecordNumber,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedPatientsResult(patients, totalCount, request.Page, request.PageSize);
    }
}
