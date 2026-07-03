using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyService.Domain.Enums;
using PharmacyService.Domain.Exceptions;
using PharmacyService.Infrastructure.Persistence;

namespace PharmacyService.Application.Pharmacy.Commands;

public record AddStockCommand(Guid MedicineId, int Quantity, string Reason) : IRequest;

public class AddStockCommandHandler : IRequestHandler<AddStockCommand>
{
    private readonly PharmacyDbContext _context;

    public AddStockCommandHandler(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == request.MedicineId, cancellationToken)
            ?? throw new MedicineNotFoundException(request.MedicineId);

        medicine.AddStock(request.Quantity);

        var movement = Domain.Entities.StockMovement.Create(
            medicine.Id, StockMovementType.In, request.Quantity, request.Reason);
        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
