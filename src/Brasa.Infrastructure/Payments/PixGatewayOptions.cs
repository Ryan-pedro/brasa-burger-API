namespace Brasa.Infrastructure.Payments;

public sealed class PixGatewayOptions
{
    public const string Section = "Payments:Pix";
    public const string FakeProvider = "Fake";

    public string? Provider { get; set; }

    public string FakeWebhookSecret { get; set; } = string.Empty;
}
