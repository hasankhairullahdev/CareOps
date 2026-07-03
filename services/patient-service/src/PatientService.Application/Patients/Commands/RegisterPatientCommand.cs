using MassTransit;
using PatientService.Domain.Entities;
using PatientService.Domain.Exceptions;
using PatientService.Domain.ValueObjects;
using PatientService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace PatientService.Application.Patients.Commands;

public record RegisterPatientCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string PhoneNumber,
    string Email,
    string Address) : IRequest<RegisterPatientResult>;

public record RegisterPatientResult(Guid Id, string MedicalRecordNumber);

public class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, RegisterPatientResult>
{
    private readonly PatientDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterPatientCommandHandler(PatientDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<RegisterPatientResult> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
    {
        var mrn = MedicalRecordNumber.Generate();

        var exists = await _context.Patients
            .AnyAsync(p => p.MedicalRecordNumber == mrn.Value, cancellationToken);

        if (exists)
            throw new DuplicateMedicalRecordException(mrn.Value);

        var patient = Patient.Create(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.PhoneNumber,
            request.Email,
            request.Address,
            mrn.Value);

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new PatientRegisteredEvent(
            patient.Id,
            patient.FirstName,
            patient.LastName,
            patient.Email,
            patient.MedicalRecordNumber,
            patient.CreatedAt), cancellationToken);

        return new RegisterPatientResult(patient.Id, patient.MedicalRecordNumber);
    }
}

public record PatientRegisteredEvent(
    Guid PatientId,
    string FirstName,
    string LastName,
    string Email,
    string MedicalRecordNumber,
    DateTime RegisteredAt);
