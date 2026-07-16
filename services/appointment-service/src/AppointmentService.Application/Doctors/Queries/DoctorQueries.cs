using AppointmentService.Domain.Exceptions;
using AppointmentService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.Doctors.Queries;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record DoctorDto(
    Guid Id,
    string Name,
    string Specialization,
    string LicenseNumber,
    string Schedule,
    string? Phone,
    string? Email,
    bool IsActive,
    DateTime CreatedAt);

// ── GetDoctors ────────────────────────────────────────────────────────────────

public record GetDoctorsQuery(
    string? Search = null,
    string? Specialization = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginatedDoctorsResult>;

public record PaginatedDoctorsResult(IReadOnlyList<DoctorDto> Items, int TotalCount);

public class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery, PaginatedDoctorsResult>
{
    private readonly AppointmentDbContext _context;
    public GetDoctorsQueryHandler(AppointmentDbContext context) => _context = context;

    public async Task<PaginatedDoctorsResult> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Doctors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(d =>
                d.Name.Contains(request.Search) ||
                d.Specialization.Contains(request.Search) ||
                d.LicenseNumber.Contains(request.Search));

        if (!string.IsNullOrWhiteSpace(request.Specialization))
            query = query.Where(d => d.Specialization == request.Specialization);

        if (request.IsActive.HasValue)
            query = query.Where(d => d.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(d => d.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DoctorDto(
                d.Id, d.Name, d.Specialization, d.LicenseNumber,
                d.Schedule, d.Phone, d.Email, d.IsActive, d.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedDoctorsResult(items, total);
    }
}

// ── GetDoctorById ─────────────────────────────────────────────────────────────

public record GetDoctorByIdQuery(Guid Id) : IRequest<DoctorDto>;

public class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, DoctorDto>
{
    private readonly AppointmentDbContext _context;
    public GetDoctorByIdQueryHandler(AppointmentDbContext context) => _context = context;

    public async Task<DoctorDto> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var d = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new DoctorNotAvailableException(request.Id, DateTime.UtcNow);

        return new DoctorDto(
            d.Id, d.Name, d.Specialization, d.LicenseNumber,
            d.Schedule, d.Phone, d.Email, d.IsActive, d.CreatedAt);
    }
}
