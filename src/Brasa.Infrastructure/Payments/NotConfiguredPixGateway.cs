using Brasa.Application.Payments.Online;

namespace Brasa.Infrastructure.Payments;

internal sealed class NotConfiguredPixGateway : IPixGateway
{
    public string Provider => "none";

    public bool IsConfigured => false;

    public Task<PixCharge> CreateChargeAsync(PixChargeRequest request, CancellationToken cancellationToken) =>
        throw new PixGatewayException("Nenhum gateway de Pix está configurado.");

    public Task<PixChargeStatus> GetChargeAsync(string chargeId, CancellationToken cancellationToken) =>
        throw new PixGatewayException("Nenhum gateway de Pix está configurado.");

    public PixWebhookNotification? ReadWebhook(PixWebhookMessage message) => null;
}
