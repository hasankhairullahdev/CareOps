using PharmacyService.Domain.Entities;
using PharmacyService.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace PharmacyService.Application.Pharmacy.Commands;

// ── Supplier CRUD ─────────────────────────────────────────────────────────────

public record CreateSupplierCommand(
    string Name, string? ContactPerson, string? Phone, string? Email, string? Address)
    : IRequest<SupplierResult>;

public record UpdateSupplierCommand(
    Guid Id, string Name, string? ContactPerson, string? Phone, string? Email, string? Address, bool IsActive)
    : IRequest<SupplierResult>;

public record DeleteSupplierCommand(Guid Id) : IRequest;

public record SupplierResult(
    Guid Id, string Name, string? ContactPerson, string? Phone, string? Email, string? Address, bool IsActive, DateTime CreatedAt);

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierResult>
{
    private readonly PharmacyDbContext _context;
    public CreateSupplierCommandHandler(PharmacyDbContext context) => _context = context;

    public async Task<SupplierResult> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = Supplier.Create(request.Name, request.ContactPerson, request.Phone, request.Email, request.Address);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        return SupplierMapper.ToResult(supplier);
    }
}

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, SupplierResult>
{
    private readonly PharmacyDbContext _context;
    public UpdateSupplierCommandHandler(PharmacyDbContext context) => _context = context;

    public async Task<SupplierResult> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Supplier '{request.Id}' not found.");

        supplier.Update(request.Name, request.ContactPerson, request.Phone, request.Email, request.Address, request.IsActive);
        await _context.SaveChangesAsync(cancellationToken);
        return SupplierMapper.ToResult(supplier);
    }
}

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand>
{
    private readonly PharmacyDbContext _context;
    public DeleteSupplierCommandHandler(PharmacyDbContext context) => _context = context;

    public async Task Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Supplier '{request.Id}' not found.");

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

internal static class SupplierMapper
{
    internal static SupplierResult ToResult(Supplier s) =>
        new(s.Id, s.Name, s.ContactPerson, s.Phone, s.Email, s.Address, s.IsActive, s.CreatedAt);
}
