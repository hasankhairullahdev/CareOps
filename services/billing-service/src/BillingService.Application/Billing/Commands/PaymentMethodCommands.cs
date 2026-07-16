using BillingService.Domain.Entities;
using BillingService.Domain.Exceptions;
using BillingService.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Application.Billing.Commands;

// ── PaymentMethod CRUD ────────────────────────────────────────────────────────

public record CreatePaymentMethodCommand(string Name, string? Description) : IRequest<PaymentMethodResult>;
public record UpdatePaymentMethodCommand(Guid Id, string Name, string? Description, bool IsActive) : IRequest<PaymentMethodResult>;
public record DeletePaymentMethodCommand(Guid Id) : IRequest;

public record PaymentMethodResult(Guid Id, string Name, string? Description, bool IsActive, DateTime CreatedAt);

public class CreatePaymentMethodCommandValidator : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300).When(x => x.Description != null);
    }
}

public class UpdatePaymentMethodCommandValidator : AbstractValidator<UpdatePaymentMethodCommand>
{
    public UpdatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300).When(x => x.Description != null);
    }
}

public class CreatePaymentMethodCommandHandler : IRequestHandler<CreatePaymentMethodCommand, PaymentMethodResult>
{
    private readonly BillingDbContext _context;
    public CreatePaymentMethodCommandHandler(BillingDbContext context) => _context = context;

    public async Task<PaymentMethodResult> Handle(CreatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await _context.PaymentMethods.AnyAsync(p => p.Name == request.Name, cancellationToken);
        if (duplicate) throw new InvalidOperationException($"Payment method '{request.Name}' already exists.");

        var entity = PaymentMethod.Create(request.Name, request.Description);
        _context.PaymentMethods.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return PaymentMethodMapper.ToResult(entity);
    }
}

public class UpdatePaymentMethodCommandHandler : IRequestHandler<UpdatePaymentMethodCommand, PaymentMethodResult>
{
    private readonly BillingDbContext _context;
    public UpdatePaymentMethodCommandHandler(BillingDbContext context) => _context = context;

    public async Task<PaymentMethodResult> Handle(UpdatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PaymentMethods
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new ServiceTariffNotFoundException(request.Id);

        var duplicate = await _context.PaymentMethods
            .AnyAsync(p => p.Name == request.Name && p.Id != request.Id, cancellationToken);
        if (duplicate) throw new InvalidOperationException($"Payment method '{request.Name}' already exists.");

        entity.Update(request.Name, request.Description, request.IsActive);
        await _context.SaveChangesAsync(cancellationToken);
        return PaymentMethodMapper.ToResult(entity);
    }
}

public class DeletePaymentMethodCommandHandler : IRequestHandler<DeletePaymentMethodCommand>
{
    private readonly BillingDbContext _context;
    public DeletePaymentMethodCommandHandler(BillingDbContext context) => _context = context;

    public async Task Handle(DeletePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PaymentMethods
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new ServiceTariffNotFoundException(request.Id);

        _context.PaymentMethods.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

internal static class PaymentMethodMapper
{
    internal static PaymentMethodResult ToResult(PaymentMethod p) =>
        new(p.Id, p.Name, p.Description, p.IsActive, p.CreatedAt);
}
