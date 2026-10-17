using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Brasa.Application.Payments.Online;
using Microsoft.Extensions.Options;

namespace Brasa.Infrastructure.Payments;

public sealed class FakePixGateway(IOptions<PixGatewayOptions> options, TimeProvider clock) : IPixGateway
{
    public const string ProviderName = "fake";
    public const string SignatureHeader = "X-Fake-Pix-Signature";

    private readonly ConcurrentDictionary<Guid, FakePixCharge> _chargesByPayment = new();
    private readonly ConcurrentDictionary<string, FakePixCharge> _chargesById = new();

    public string Provider => ProviderName;

    public bool IsConfigured => true;

    public Task<PixCharge> CreateChargeAsync(PixChargeRequest request, CancellationToken cancellationToken)
    {
        var charge = _chargesByPayment.GetOrAdd(request.PaymentId, _ => new FakePixCharge($"fake_{Guid.NewGuid():N}", request.Amount, request.ExpiresAt));
        _chargesById.TryAdd(charge.ChargeId, charge);
        return Task.FromResult(new PixCharge(charge.ChargeId, CopyPaste(charge)));
    }

    public Task<PixChargeStatus> GetChargeAsync(string chargeId, CancellationToken cancellationToken)
    {
        if (!_chargesById.TryGetValue(chargeId, out var charge))
        {
            throw new PixGatewayException($"Cobrança {chargeId} desconhecida pelo gateway falso.");
        }

        var state = charge.PaidAmount is not null ? PixChargeState.Paid
            : clock.GetUtcNow() >= charge.ExpiresAt ? PixChargeState.Expired
            : PixChargeState.Pending;

        return Task.FromResult(new PixChargeStatus(chargeId, state, charge.PaidAmount, "BRL"));
    }

    public PixWebhookNotification? ReadWebhook(PixWebhookMessage message)
    {
        if (!message.Headers.TryGetValue(SignatureHeader, out var signature)
            || !CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(signature), Encoding.ASCII.GetBytes(Sign(message.Body))))
        {
            return null;
        }

        var body = JsonSerializer.Deserialize<FakePixWebhookBody>(message.Body, JsonSerializerOptions.Web);
        return body is { EventId.Length: > 0, ChargeId.Length: > 0 } ? new PixWebhookNotification(body.EventId, body.ChargeId) : null;
    }

    public PixWebhookMessage SimulatePayment(string chargeId, decimal? paidAmount)
    {
        if (!_chargesById.TryGetValue(chargeId, out var charge))
        {
            throw new PixGatewayException($"Cobrança {chargeId} desconhecida pelo gateway falso.");
        }

        charge.PaidAmount = paidAmount ?? charge.Amount;

        var body = JsonSerializer.Serialize(new FakePixWebhookBody($"evt_{Guid.NewGuid():N}", chargeId), JsonSerializerOptions.Web);
        return new PixWebhookMessage(body, new Dictionary<string, string> { [SignatureHeader] = Sign(body) });
    }

    private string Sign(string body)
    {
        var secret = options.Value.FakeWebhookSecret;
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("Configure Payments:Pix:FakeWebhookSecret para usar o gateway falso.");
        }

        return Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body)));
    }

    private static string CopyPaste(FakePixCharge charge) =>
        $"00020126580014BR.GOV.BCB.PIX0136{charge.ChargeId}5204000053039865406{charge.Amount.ToString("0.00", CultureInfo.InvariantCulture)}5802BR5913BRASA BURGER6009SAO PAULO6304FAKE";
}
