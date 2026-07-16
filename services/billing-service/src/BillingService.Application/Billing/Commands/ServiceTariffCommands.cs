using BillingService.Domain.Exceptions;
using BillingService.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Application.Billing.Commands;

// ── ServiceTariff Commands ────────────────────────────────────────────────────

public record CreateServiceTariffCommand(
    string ServiceName,
    string Category,
    decimal Price,
    string? Description) : IRequest<ServiceTariffResult>;

public record ServiceTariffResult(
    Guid Id, string ServiceName, string Category,
    decimal Price, string? Description, bool IsActive, DateTime CreatedAt);

public class CreateServiceTariffCommandValidator : AbstractValidator<CreateServiceTariffCommand>
{
    public CreateServiceTariffCommandValidator()
    {
        RuleFor(x => x.ServiceName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class CreateServiceTariffCommandHandler : IRequestHandler<CreateServiceTariffCommand, ServiceTariffResult>
{
    private readonly BillingDbContext _context;
    public CreateServiceTariffCommandHandler(BillingDbContext context) => _context = context;

    public async Task<ServiceTariffResult> Handle(CreateServiceTariffCommand request, CancellationToken cancellationToken)
    {
        var tariff = Domain.Entities.ServiceTariff.Create(
            request.ServiceName, request.Category, request.Price, request.Description);

        _context.ServiceTariffs.Add(tariff);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceTariffMapper.ToResult(tariff);
    }
}

// ── Update ServiceTariff ──────────────────────────────────────────────────────

public record UpdateServiceTariffCommand(
    Guid Id,
    string ServiceName,
    string Category,
    decimal Price,
    string? Description,
    bool IsActive) : IRequest<ServiceTariffResult>;

public class UpdateServiceTariffCommandValidator : AbstractValidator<UpdateServiceTariffCommand>
{
    public UpdateServiceTariffCommandValidator()
    {
        RuleFor(x => x.ServiceName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class UpdateServiceTariffCommandHandler : IRequestHandler<UpdateServiceTariffCommand, ServiceTariffResult>
{
    private readonly BillingDbContext _context;
    public UpdateServiceTariffCommandHandler(BillingDbContext context) => _context = context;

    public async Task<ServiceTariffResult> Handle(UpdateServiceTariffCommand request, CancellationToken cancellationToken)
    {
        var tariff = await _context.ServiceTariffs
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new ServiceTariffNotFoundException(request.Id);

        tariff.Update(request.ServiceName, request.Category, request.Price, request.Description, request.IsActive);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceTariffMapper.ToResult(tariff);
    }
}

// ── Delete ServiceTariff ──────────────────────────────────────────────────────

public record DeleteServiceTariffCommand(Guid Id) : IRequest;

public class DeleteServiceTariffCommandHandler : IRequestHandler<DeleteServiceTariffCommand>
{
    private readonly BillingDbContext _context;
    public DeleteServiceTariffCommandHandler(BillingDbContext context) => _context = context;

    public async Task Handle(DeleteServiceTariffCommand request, CancellationToken cancellationToken)
    {
        var tariff = await _context.ServiceTariffs
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new ServiceTariffNotFoundException(request.Id);

        _context.ServiceTariffs.Remove(tariff);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

// ── Shared helper ─────────────────────────────────────────────────────────────

internal static class ServiceTariffMapper
{
    internal static ServiceTariffResult ToResult(Domain.Entities.ServiceTariff t) =>
        new(t.Id, t.ServiceName, t.Category, t.Price, t.Description, t.IsActive, t.CreatedAt);
}
