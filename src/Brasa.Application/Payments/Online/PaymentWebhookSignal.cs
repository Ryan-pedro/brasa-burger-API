using Brasa.Application.Shared.Jobs;

namespace Brasa.Application.Payments.Online;

public sealed class PaymentWebhookSignal() : JobSignal(FallbackInterval)
{
    public static readonly TimeSpan FallbackInterval = TimeSpan.FromSeconds(30);
}
