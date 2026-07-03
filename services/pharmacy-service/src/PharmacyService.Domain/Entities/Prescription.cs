using PharmacyService.Domain.Enums;

namespace PharmacyService.Domain.Entities;

public class Prescription
{
    public Guid Id { get; private set; }
    public Guid ExternalPrescriptionId { get; private set; } // from appointment-service
    public Guid PatientId { get; private set; }
    public Guid AppointmentId { get; private set; }
    public PrescriptionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DispensedAt { get; private set; }
    public List<PrescriptionItem> Items { get; private set; } = new();

    private Prescription() { }

    public static Prescription CreateFromEvent(
        Guid externalPrescriptionId,
        Guid patientId,
        Guid appointmentId,
        List<PrescriptionItem> items)
    {
        return new Prescription
        {
            Id = Guid.NewGuid(),
            ExternalPrescriptionId = externalPrescriptionId,
            PatientId = patientId,
            AppointmentId = appointmentId,
            Status = PrescriptionStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = items
        };
    }

    public void Dispense()
    {
        if (Status == PrescriptionStatus.Dispensed)
            throw new PharmacyService.Domain.Exceptions.PrescriptionAlreadyDispensedException(Id);
        Status = PrescriptionStatus.Dispensed;
        DispensedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = PrescriptionStatus.Cancelled;
    }
}

public class PrescriptionItem
{
    public Guid Id { get; private set; }
    public Guid PrescriptionId { get; private set; }
    public Guid? MedicineId { get; private set; }
    public string MedicineName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public string Dosage { get; private set; } = string.Empty;
    public string Instructions { get; private set; } = string.Empty;

    private PrescriptionItem() { }

    public static PrescriptionItem Create(
        Guid prescriptionId,
        string medicineName,
        int quantity,
        string dosage,
        string instructions,
        Guid? medicineId = null)
    {
        return new PrescriptionItem
        {
            Id = Guid.NewGuid(),
            PrescriptionId = prescriptionId,
            MedicineId = medicineId,
            MedicineName = medicineName,
            Quantity = quantity,
            Dosage = dosage,
            Instructions = instructions
        };
    }
}
