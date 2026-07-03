using PatientService.Domain.Exceptions;
using PatientService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace PatientService.Application.Patients.Commands;

public record UpdatePatientCommand(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string PhoneNumber,
    string Email,
    string Address) : IRequest;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand>
{
    private readonly PatientDbContext _context;

    public UpdatePatientCommandHandler(PatientDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new PatientNotFoundException(request.Id);

        patient.Update(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.PhoneNumber,
            request.Email,
            request.Address);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
