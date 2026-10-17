namespace Brasa.Api.Http;

internal sealed class RateLimitSettings
{
    public const string Section = "RateLimits";

    public int GlobalPerMinute { get; set; } = 600;

    public int OrdersPerMinute { get; set; } = 10;

    public int QuotesPerMinute { get; set; } = 60;

    public int OrderLookupsPerMinute { get; set; } = 60;

    public int LoginPerMinute { get; set; } = 10;

    public int AgentCredentialsPerMinute { get; set; } = 10;

    public int InvitationsPerMinute { get; set; } = 20;

    public int WebhooksPerMinute { get; set; } = 300;
}
