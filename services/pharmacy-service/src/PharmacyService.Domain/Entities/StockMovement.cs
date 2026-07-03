using PharmacyService.Domain.Enums;

namespace PharmacyService.Domain.Entities;

public class StockMovement
{
    public Guid Id { get; private set; }
    public Guid MedicineId { get; private set; }
    public StockMovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private StockMovement() { }

    public static StockMovement Create(Guid medicineId, StockMovementType type, int quantity, string reason)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            MedicineId = medicineId,
            Type = type,
            Quantity = quantity,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
    }
}
