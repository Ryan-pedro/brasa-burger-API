namespace Brasa.Api.Http;

internal sealed class RateLimitSettings
{
    public const string Section = "RateLimits";

    public int OrdersPerMinute { get; set; } = 10;

    public int QuotesPerMinute { get; set; } = 60;

    public int AgentCredentialsPerMinute { get; set; } = 10;
}
