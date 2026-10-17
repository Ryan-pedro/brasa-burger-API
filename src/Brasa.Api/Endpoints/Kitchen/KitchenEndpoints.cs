using Brasa.Api.Auth;
using Brasa.Api.Problems;
using Brasa.Application.Kitchen.Board;
using Brasa.Application.Kitchen.Tickets;
using Brasa.Application.Printing.Jobs;
using Permissions = Brasa.Application.Identity.Permissions;

namespace Brasa.Api.Endpoints.Kitchen;

internal static class KitchenEndpoints
{
    public static IEndpointRouteBuilder MapKitchenEndpoints(this IEndpointRouteBuilder api)
    {
        var kitchen = api.MapGroup("/kitchen")
            .AddEndpointFilter<CsrfHeaderFilter>()
            .RequireStaffSession();

        kitchen.MapGet("/tickets", async (GetBoardHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .RequirePermission(Permissions.KitchenTicketsRead)
            .WithName("KitchenBoard");

        kitchen.MapGet("/tickets/{id:guid}", async (Guid id, GetTicketHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.KitchenTicketsRead)
            .WithName("KitchenGetTicket");

        MapMove(kitchen, "advance", PreparationMove.Advance);
        MapMove(kitchen, "undo", PreparationMove.Undo);

        kitchen.MapPost("/tickets/{id:guid}/reprint", async (Guid id, ReprintTicketHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.KitchenTicketsReprint)
            .WithName("KitchenReprintTicket");

        return api;
    }

    private static void MapMove(RouteGroupBuilder kitchen, string route, PreparationMove move) =>
        kitchen.MapPost($"/tickets/{{id:guid}}/{route}", async (Guid id, PreparationMoveRequest request, MovePreparationHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, move, request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.KitchenTicketsUpdate)
            .WithName($"Kitchen{move}Ticket");
}
