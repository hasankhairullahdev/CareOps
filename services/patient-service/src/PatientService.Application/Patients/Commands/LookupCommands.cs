using PatientService.Domain.Entities;
using PatientService.Domain.Exceptions;
using PatientService.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace PatientService.Application.Patients.Commands;

// ── AllergyType CRUD ──────────────────────────────────────────────────────────

public record CreateAllergyTypeCommand(string Name, string? Description) : IRequest<AllergyTypeResult>;
public record UpdateAllergyTypeCommand(Guid Id, string Name, string? Description, bool IsActive) : IRequest<AllergyTypeResult>;
public record DeleteAllergyTypeCommand(Guid Id) : IRequest;

public record AllergyTypeResult(Guid Id, string Name, string? Description, bool IsActive, DateTime CreatedAt);

public class CreateAllergyTypeCommandValidator : AbstractValidator<CreateAllergyTypeCommand>
{
    public CreateAllergyTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300).When(x => x.Description != null);
    }
}

public class UpdateAllergyTypeCommandValidator : AbstractValidator<UpdateAllergyTypeCommand>
{
    public UpdateAllergyTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300).When(x => x.Description != null);
    }
}

public class CreateAllergyTypeCommandHandler : IRequestHandler<CreateAllergyTypeCommand, AllergyTypeResult>
{
    private readonly PatientDbContext _context;
    public CreateAllergyTypeCommandHandler(PatientDbContext context) => _context = context;

    public async Task<AllergyTypeResult> Handle(CreateAllergyTypeCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await _context.AllergyTypes.AnyAsync(a => a.Name == request.Name, cancellationToken);
        if (duplicate) throw new InvalidOperationException($"Allergy type '{request.Name}' already exists.");

        var entity = AllergyType.Create(request.Name, request.Description);
        _context.AllergyTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return AllergyTypeMapper.ToResult(entity);
    }
}

public class UpdateAllergyTypeCommandHandler : IRequestHandler<UpdateAllergyTypeCommand, AllergyTypeResult>
{
    private readonly PatientDbContext _context;
    public UpdateAllergyTypeCommandHandler(PatientDbContext context) => _context = context;

    public async Task<AllergyTypeResult> Handle(UpdateAllergyTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AllergyTypes.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new PatientNotFoundException(request.Id);

        var duplicate = await _context.AllergyTypes.AnyAsync(a => a.Name == request.Name && a.Id != request.Id, cancellationToken);
        if (duplicate) throw new InvalidOperationException($"Allergy type '{request.Name}' already exists.");

        entity.Update(request.Name, request.Description, request.IsActive);
        await _context.SaveChangesAsync(cancellationToken);
        return AllergyTypeMapper.ToResult(entity);
    }
}

public class DeleteAllergyTypeCommandHandler : IRequestHandler<DeleteAllergyTypeCommand>
{
    private readonly PatientDbContext _context;
    public DeleteAllergyTypeCommandHandler(PatientDbContext context) => _context = context;

    public async Task Handle(DeleteAllergyTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AllergyTypes.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new PatientNotFoundException(request.Id);

        _context.AllergyTypes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

internal static class AllergyTypeMapper
{
    internal static AllergyTypeResult ToResult(AllergyType a) =>
        new(a.Id, a.Name, a.Description, a.IsActive, a.CreatedAt);
}
