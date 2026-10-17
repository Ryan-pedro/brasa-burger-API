using System.Globalization;
using System.Threading.RateLimiting;
using Brasa.Api.Problems;

namespace Brasa.Api.Http;

internal static class RateLimits
{
    public const string Orders = "orders";
    public const string Quotes = "quotes";
    public const string OrderLookups = "order-lookups";
    public const string Login = "login";
    public const string AgentCredentials = "agent-credentials";
    public const string Invitations = "invitations";
    public const string Webhooks = "webhooks";

    private static readonly PathString[] PathsWithoutGlobalLimit = ["/api/v1/health", "/api/v1/hubs"];

    public static IServiceCollection AddPublicRateLimits(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitSettings.Section).Get<RateLimitSettings>() ?? new RateLimitSettings();

        return services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                IsExemptFromGlobalLimit(http.Request.Path)
                    ? RateLimitPartition.GetNoLimiter(string.Empty)
                    : PerClient(http, settings.GlobalPerMinute));

            options.AddPolicy(Orders, http => PerClient(http, settings.OrdersPerMinute));
            options.AddPolicy(Quotes, http => PerClient(http, settings.QuotesPerMinute));
            options.AddPolicy(OrderLookups, http => PerClient(http, settings.OrderLookupsPerMinute));
            options.AddPolicy(Login, http => PerClient(http, settings.LoginPerMinute));
            options.AddPolicy(AgentCredentials, http => PerClient(http, settings.AgentCredentialsPerMinute));
            options.AddPolicy(Invitations, http => PerClient(http, settings.InvitationsPerMinute));
            options.AddPolicy(Webhooks, http => PerClient(http, settings.WebhooksPerMinute));

            options.OnRejected = (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                return new ValueTask(ApiProblems
                    .Create(StatusCodes.Status429TooManyRequests, "Muitas tentativas. Aguarde um minuto.", "rate_limited")
                    .ExecuteAsync(context.HttpContext));
            };
        });
    }

    private static bool IsExemptFromGlobalLimit(PathString path) =>
        PathsWithoutGlobalLimit.Any(exempt => path.StartsWithSegments(exempt));

    private static RateLimitPartition<string> PerClient(HttpContext http, int permitsPerMinute) =>
        RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permitsPerMinute, Window = TimeSpan.FromMinutes(1) });
}
