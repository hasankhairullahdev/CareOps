using AppointmentService.Domain.Enums;
using AppointmentService.Domain.Exceptions;
using AppointmentService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.Appointments.Queries;

public record GetAppointmentByIdQuery(Guid Id) : IRequest<AppointmentDto>;

public record AppointmentDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    string DoctorName,
    DateTime ScheduledAt,
    AppointmentStatus Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto>
{
    private readonly AppointmentDbContext _context;

    public GetAppointmentByIdQueryHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<AppointmentDto> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .Where(a => a.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new AppointmentNotFoundException(request.Id);

        return result;
    }
}
