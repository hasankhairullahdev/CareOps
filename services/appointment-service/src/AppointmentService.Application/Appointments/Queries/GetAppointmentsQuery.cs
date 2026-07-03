using AppointmentService.Domain.Enums;
using AppointmentService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.Appointments.Queries;

public record GetAppointmentsQuery(
    Guid? DoctorId = null,
    Guid? PatientId = null,
    DateOnly? Date = null,
    AppointmentStatus? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginatedAppointmentsResult>;

public record PaginatedAppointmentsResult(
    IReadOnlyList<AppointmentDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, PaginatedAppointmentsResult>
{
    private readonly AppointmentDbContext _context;

    public GetAppointmentsQueryHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedAppointmentsResult> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .AsQueryable();

        if (request.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == request.DoctorId.Value);

        if (request.PatientId.HasValue)
            query = query.Where(a => a.PatientId == request.PatientId.Value);

        if (request.Date.HasValue)
        {
            var date = request.Date.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(a => a.ScheduledAt.Date == date.Date);
        }

        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var appointments = await query
            .OrderByDescending(a => a.ScheduledAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AppointmentDto(
                a.Id,
                a.PatientId,
                a.DoctorId,
                a.Doctor!.Name,
                a.ScheduledAt,
                a.Status,
                a.Notes,
                a.CreatedAt,
                a.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedAppointmentsResult(appointments, totalCount, request.Page, request.PageSize);
    }
}
