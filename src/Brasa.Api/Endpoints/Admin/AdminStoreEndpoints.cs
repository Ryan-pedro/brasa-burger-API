using Brasa.Api.Auth;
using Brasa.Api.Problems;
using Brasa.Application.StoreSettings.Settings;
using Permissions = Brasa.Application.Identity.Permissions;

namespace Brasa.Api.Endpoints.Admin;

internal static class AdminStoreEndpoints
{
    public static RouteGroupBuilder MapAdminStoreEndpoints(this RouteGroupBuilder secured)
    {
        var store = secured.MapGroup("/store").RequirePermission(Permissions.StoreSettings);

        store.MapGet("/settings", async (GetStoreSettingsHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminGetStoreSettings");

        store.MapPut("/settings", async (StoreSettingsRequest request, UpdateStoreSettingsHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminUpdateStoreSettings");

        store.MapPut("/status", async (StoreStatusRequest request, SetStoreStatusHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminSetStoreStatus");

        return secured;
    }
}
