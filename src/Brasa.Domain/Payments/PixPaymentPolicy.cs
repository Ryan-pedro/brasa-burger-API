namespace Brasa.Domain.Payments;

public static class PixPaymentPolicy
{
    public static readonly TimeSpan PaymentWindow = TimeSpan.FromMinutes(15);

    public static readonly TimeSpan ReconcileAfter = TimeSpan.FromMinutes(1);

    public const string Currency = "BRL";
}
