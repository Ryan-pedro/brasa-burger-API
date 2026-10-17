namespace Brasa.Api.Realtime;

internal static class RealtimeEndpoints
{
    public static IEndpointRouteBuilder MapRealtimeHubs(this IEndpointRouteBuilder api)
    {
        api.MapHub<KitchenHub>("/hubs/kitchen");
        api.MapHub<OpsHub>("/hubs/ops");
        api.MapHub<AgentHub>("/hubs/agent");
        return api;
    }
}
