namespace Brasa.Application.Payments.Online;

public interface IPixGateway
{
    string Provider { get; }

    bool IsConfigured { get; }

    Task<PixCharge> CreateChargeAsync(PixChargeRequest request, CancellationToken cancellationToken);

    Task<PixChargeStatus> GetChargeAsync(string chargeId, CancellationToken cancellationToken);

    PixWebhookNotification? ReadWebhook(PixWebhookMessage message);
}
