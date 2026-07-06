using BillingService.Domain.Enums;
using BillingService.Domain.Exceptions;

namespace BillingService.Domain.Entities;

public class Bill
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid AppointmentId { get; private set; }
    public BillStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public List<BillLineItem> LineItems { get; private set; } = new();

    private Bill() { }

    public static Bill Create(Guid patientId, Guid appointmentId, string consultationDescription, decimal consultationFee)
    {
        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            AppointmentId = appointmentId,
            Status = BillStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        bill.LineItems.Add(BillLineItem.Create(bill.Id, consultationDescription, 1, consultationFee));
        bill.RecalculateTotal();
        return bill;
    }

    public void AddLineItem(string description, int quantity, decimal unitPrice)
    {
        LineItems.Add(BillLineItem.Create(Id, description, quantity, unitPrice));
        RecalculateTotal();
    }

    public void Issue()
    {
        if (Status == BillStatus.Paid)
            throw new BillAlreadyPaidException(Id);
        if (Status == BillStatus.Cancelled)
            throw new InvalidOperationException($"Bill '{Id}' is cancelled and cannot be issued.");

        Status = BillStatus.Issued;
        IssuedAt = DateTime.UtcNow;
        RecalculateTotal();
    }

    public void Pay()
    {
        if (Status == BillStatus.Paid)
            throw new BillAlreadyPaidException(Id);
        if (Status != BillStatus.Issued)
            throw new BillNotIssuedException(Id);

        Status = BillStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == BillStatus.Paid)
            throw new BillAlreadyPaidException(Id);

        Status = BillStatus.Cancelled;
    }

    private void RecalculateTotal()
    {
        TotalAmount = Math.Round(LineItems.Sum(i => i.Amount), 0, MidpointRounding.AwayFromZero);
    }
}

public class BillLineItem
{
    public Guid Id { get; private set; }
    public Guid BillId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Amount { get; private set; }

    private BillLineItem() { }

    public static BillLineItem Create(Guid billId, string description, int quantity, decimal unitPrice)
    {
        return new BillLineItem
        {
            Id = Guid.NewGuid(),
            BillId = billId,
            Description = description,
            Quantity = quantity,
            UnitPrice = Math.Round(unitPrice, 2),
            Amount = Math.Round(unitPrice * quantity, 0, MidpointRounding.AwayFromZero)
        };
    }
}
