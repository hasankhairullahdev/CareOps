using AppointmentService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.Appointments.Queries;

public record GetDoctorScheduleQuery(Guid DoctorId, DateOnly Date) : IRequest<DoctorScheduleDto>;

public record DoctorScheduleDto(
    Guid DoctorId,
    string DoctorName,
    string Schedule,
    IReadOnlyList<AppointmentDto> Appointments);

public class GetDoctorScheduleQueryHandler : IRequestHandler<GetDoctorScheduleQuery, DoctorScheduleDto>
{
    private readonly AppointmentDbContext _context;

    public GetDoctorScheduleQueryHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorScheduleDto> Handle(GetDoctorScheduleQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken)
            ?? throw new AppointmentService.Domain.Exceptions.DoctorNotAvailableException(request.DoctorId, request.Date.ToDateTime(TimeOnly.MinValue));

        var date = request.Date.ToDateTime(TimeOnly.MinValue);

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .Where(a => a.DoctorId == request.DoctorId && a.ScheduledAt.Date == date.Date)
            .OrderBy(a => a.ScheduledAt)
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

        return new DoctorScheduleDto(doctor.Id, doctor.Name, doctor.Schedule, appointments);
    }
}
