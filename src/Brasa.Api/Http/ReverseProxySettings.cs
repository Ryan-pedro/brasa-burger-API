using Microsoft.AspNetCore.HttpOverrides;

namespace Brasa.Api.Http;

internal static class ReverseProxySettings
{
    public const string TrustedNetworksSetting = "ReverseProxy:TrustedNetworks";

    public static IServiceCollection AddTrustedReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        var networks = configuration.GetSection(TrustedNetworksSetting).Get<string[]>() ?? [];

        return services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.ForwardLimit = 1;
            foreach (var network in networks)
            {
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
            }
        });
    }
}
