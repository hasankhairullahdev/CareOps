using FluentValidation;
using PharmacyService.Application.Pharmacy.Commands;

namespace PharmacyService.Application.Pharmacy.Commands;

public class DispensePrescriptionCommandValidator : AbstractValidator<DispensePrescriptionCommand>
{
    public DispensePrescriptionCommandValidator()
    {
        RuleFor(x => x.PrescriptionId).NotEmpty();
    }
}

public class AddStockCommandValidator : AbstractValidator<AddStockCommand>
{
    public AddStockCommandValidator()
    {
        RuleFor(x => x.MedicineId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
