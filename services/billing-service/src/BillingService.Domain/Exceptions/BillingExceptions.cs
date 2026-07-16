namespace BillingService.Domain.Exceptions;

public sealed class BillNotFoundException : Exception
{
    public BillNotFoundException(Guid billId)
        : base($"Bill with ID '{billId}' was not found.") { }
}

public sealed class BillAlreadyPaidException : Exception
{
    public BillAlreadyPaidException(Guid billId)
        : base($"Bill '{billId}' has already been paid.") { }

}

public sealed class BillNotIssuedException : Exception
{
    public BillNotIssuedException(Guid billId)
        : base($"Bill '{billId}' must be in Issued status before payment.") { }
}

public sealed class ServiceTariffNotFoundException : Exception
{
    public ServiceTariffNotFoundException(Guid tariffId)
        : base($"Service tariff with ID '{tariffId}' was not found.") { }
}
