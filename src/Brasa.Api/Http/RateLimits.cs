using System.Threading.RateLimiting;
using Brasa.Api.Problems;
using Microsoft.AspNetCore.RateLimiting;

namespace Brasa.Api.Http;

internal sealed class RateLimitSettings
{
    public const string Section = "RateLimits";

    public int OrdersPerMinute { get; set; } = 10;

    public int QuotesPerMinute { get; set; } = 60;
}

internal static class RateLimits
{
    public const string Orders = "orders";
    public const string Quotes = "quotes";

    public static IServiceCollection AddPublicRateLimits(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitSettings.Section).Get<RateLimitSettings>() ?? new RateLimitSettings();

        return services.AddRateLimiter(options =>
        {
            options.AddPolicy(Orders, http => PerClient(http, settings.OrdersPerMinute));
            options.AddPolicy(Quotes, http => PerClient(http, settings.QuotesPerMinute));
            options.OnRejected = (context, _) =>
                new ValueTask(ApiProblems
                    .Create(StatusCodes.Status429TooManyRequests, "Muitas tentativas. Aguarde um minuto.", "rate_limited")
                    .ExecuteAsync(context.HttpContext));
        });
    }

    private static RateLimitPartition<string> PerClient(HttpContext http, int permitsPerMinute) =>
        RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permitsPerMinute, Window = TimeSpan.FromMinutes(1) });
}
