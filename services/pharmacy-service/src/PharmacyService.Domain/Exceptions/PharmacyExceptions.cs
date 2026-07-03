namespace PharmacyService.Domain.Exceptions;

public sealed class InsufficientStockException : Exception
{
    public InsufficientStockException(Guid medicineId, string medicineName, int requested, int available)
        : base($"Insufficient stock for '{medicineName}' (ID: {medicineId}). Requested: {requested}, Available: {available}.")
    {
    }
}

public sealed class MedicineNotFoundException : Exception
{
    public MedicineNotFoundException(Guid medicineId)
        : base($"Medicine with ID '{medicineId}' was not found.")
    {
    }
}

public sealed class PrescriptionNotFoundException : Exception
{
    public PrescriptionNotFoundException(Guid prescriptionId)
        : base($"Prescription with ID '{prescriptionId}' was not found.")
    {
    }
}

public sealed class PrescriptionAlreadyDispensedException : Exception
{
    public PrescriptionAlreadyDispensedException(Guid prescriptionId)
        : base($"Prescription '{prescriptionId}' has already been dispensed.")
    {
    }
}
