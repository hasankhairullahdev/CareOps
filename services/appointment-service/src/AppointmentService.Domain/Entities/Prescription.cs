namespace AppointmentService.Domain.Entities;

public class Prescription
{
    public Guid Id { get; private set; }
    public Guid AppointmentId { get; private set; }
    public Guid PatientId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<PrescriptionItem> Items { get; private set; } = new();

    private Prescription() { }

    public static Prescription Create(Guid appointmentId, Guid patientId, List<PrescriptionItem> items)
    {
        return new Prescription
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointmentId,
            PatientId = patientId,
            CreatedAt = DateTime.UtcNow,
            Items = items
        };
    }
}

public class PrescriptionItem
{
    public Guid Id { get; private set; }
    public Guid PrescriptionId { get; private set; }
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
        string instructions)
    {
        return new PrescriptionItem
        {
            Id = Guid.NewGuid(),
            PrescriptionId = prescriptionId,
            MedicineName = medicineName,
            Quantity = quantity,
            Dosage = dosage,
            Instructions = instructions
        };
    }
}
