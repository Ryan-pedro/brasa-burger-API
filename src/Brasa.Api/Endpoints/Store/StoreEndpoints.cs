using Brasa.Api.Http;
using Brasa.Api.Problems;
using Brasa.Application.Catalog.Menu;
using Brasa.Application.Catalog.Offers;
using Brasa.Application.Ordering.Checkout;
using Brasa.Application.Ordering.Tracking;
using Brasa.Application.StoreSettings.GetStoreProfile;
using Microsoft.AspNetCore.Mvc;

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

        store.MapGet("/menu", async (GetMenuHandler handler, HttpContext http, CancellationToken cancellationToken) =>
                JsonWithETag.Create(http, await handler.HandleAsync(cancellationToken)))
            .WithName("GetMenu");

        store.MapGet("/offers", async (ListActiveOffersHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .WithName("GetActiveOffers");

        store.MapPost("/orders/quote", async (QuoteRequest request, QuoteHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequireRateLimiting(RateLimits.Quotes)
            .WithName("QuoteOrder");

        store.MapPost("/orders", async (
                [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
                CreateOrderRequest request,
                CreateOrderHandler handler,
                CancellationToken cancellationToken) =>
                (await handler.HandleAsync(idempotencyKey, request, cancellationToken)).Match(created => created.Replayed
                    ? Results.Ok(created.Order)
                    : Results.Created($"/api/v1/store/orders/{created.Order.TrackingToken}", created.Order)))
            .RequireRateLimiting(RateLimits.Orders)
            .WithName("CreateOrder");

        store.MapGet("/orders/{token}", async (string token, GetOrderTrackingHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(token, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("TrackOrder");

        return api;
    }
}
