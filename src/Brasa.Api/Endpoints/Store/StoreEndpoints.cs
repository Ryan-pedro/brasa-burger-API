using Brasa.Api.Problems;
using Brasa.Application.Catalog.Menu;
using Brasa.Application.StoreSettings.GetStoreProfile;

namespace Brasa.Api.Endpoints.Store;

/// <summary>Superfície pública: o que o Storefront pode chamar.</summary>
public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStoreEndpoints(this IEndpointRouteBuilder api)
    {
        var store = api.MapGroup("/store");

        store.MapGet("/profile", async (GetStoreProfileHandler handler, CancellationToken cancellationToken) =>
            {
                var profile = await handler.HandleAsync(cancellationToken);
                return profile is null
                    ? ApiProblems.Create(StatusCodes.Status503ServiceUnavailable, "A loja ainda não foi configurada.", "store_not_configured")
                    : Results.Ok(profile);
            })
            .WithName("GetStoreProfile");

        store.MapGet("/menu", async (GetMenuHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .WithName("GetMenu");

        return api;
    }
}
