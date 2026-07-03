using PatientService.Domain.Exceptions;
using PatientService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace PatientService.Application.Patients.Queries;

public record GetPatientByIdQuery(Guid Id) : IRequest<PatientDto>;

public record PatientDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string PhoneNumber,
    string Email,
    string Address,
    string MedicalRecordNumber,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, PatientDto>
{
    private readonly PatientDbContext _context;

    public GetPatientByIdQueryHandler(PatientDbContext context)
    {
        _context = context;
    }

    public async Task<PatientDto> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new PatientNotFoundException(request.Id);

        return new PatientDto(
            patient.Id,
            patient.FirstName,
            patient.LastName,
            patient.DateOfBirth,
            patient.Gender,
            patient.PhoneNumber,
            patient.Email,
            patient.Address,
            patient.MedicalRecordNumber,
            patient.CreatedAt,
            patient.UpdatedAt);
    }
}
