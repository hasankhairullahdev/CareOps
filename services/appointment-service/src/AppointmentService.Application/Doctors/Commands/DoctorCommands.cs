using AppointmentService.Domain.Entities;
using AppointmentService.Domain.Exceptions;
using AppointmentService.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.Doctors.Commands;

// ── Create Doctor ─────────────────────────────────────────────────────────────

public record CreateDoctorCommand(
    string Name,
    string Specialization,
    string LicenseNumber,
    string Schedule,
    string? Phone,
    string? Email) : IRequest<CreateDoctorResult>;

public record CreateDoctorResult(Guid Id, string Name, string Specialization, string LicenseNumber,
    string Schedule, string? Phone, string? Email, bool IsActive, DateTime CreatedAt);

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Schedule).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, CreateDoctorResult>
{
    private readonly AppointmentDbContext _context;
    public CreateDoctorCommandHandler(AppointmentDbContext context) => _context = context;

    public async Task<CreateDoctorResult> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await _context.Doctors
            .AnyAsync(d => d.LicenseNumber == request.LicenseNumber, cancellationToken);
        if (duplicate)
            throw new InvalidOperationException($"A doctor with license number '{request.LicenseNumber}' already exists.");

        var doctor = Doctor.Create(
            request.Name, request.Specialization, request.LicenseNumber,
            request.Schedule, request.Phone, request.Email);

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateDoctorResult(
            doctor.Id, doctor.Name, doctor.Specialization, doctor.LicenseNumber,
            doctor.Schedule, doctor.Phone, doctor.Email, doctor.IsActive, doctor.CreatedAt);
    }
}

// ── Update Doctor ─────────────────────────────────────────────────────────────

public record UpdateDoctorCommand(
    Guid Id,
    string Name,
    string Specialization,
    string LicenseNumber,
    string Schedule,
    string? Phone,
    string? Email,
    bool IsActive) : IRequest<UpdateDoctorResult>;

public record UpdateDoctorResult(Guid Id, string Name, string Specialization, string LicenseNumber,
    string Schedule, string? Phone, string? Email, bool IsActive, DateTime CreatedAt);

public class UpdateDoctorCommandValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Schedule).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

public class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand, UpdateDoctorResult>
{
    private readonly AppointmentDbContext _context;
    public UpdateDoctorCommandHandler(AppointmentDbContext context) => _context = context;

    public async Task<UpdateDoctorResult> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new DoctorNotAvailableException(request.Id, DateTime.UtcNow);

        var duplicate = await _context.Doctors
            .AnyAsync(d => d.LicenseNumber == request.LicenseNumber && d.Id != request.Id, cancellationToken);
        if (duplicate)
            throw new InvalidOperationException($"A doctor with license number '{request.LicenseNumber}' already exists.");

        doctor.Update(request.Name, request.Specialization, request.LicenseNumber,
            request.Schedule, request.Phone, request.Email, request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateDoctorResult(
            doctor.Id, doctor.Name, doctor.Specialization, doctor.LicenseNumber,
            doctor.Schedule, doctor.Phone, doctor.Email, doctor.IsActive, doctor.CreatedAt);
    }
}

// ── Delete Doctor ─────────────────────────────────────────────────────────────

public record DeleteDoctorCommand(Guid Id) : IRequest;

public class DeleteDoctorCommandHandler : IRequestHandler<DeleteDoctorCommand>
{
    private readonly AppointmentDbContext _context;
    public DeleteDoctorCommandHandler(AppointmentDbContext context) => _context = context;

    public async Task Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new DoctorNotAvailableException(request.Id, DateTime.UtcNow);

        var hasAppointments = await _context.Appointments
            .AnyAsync(a => a.DoctorId == request.Id, cancellationToken);
        if (hasAppointments)
            throw new InvalidOperationException($"Cannot delete doctor '{request.Id}' because they have existing appointments.");

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
