using Brasa.Api.Auth;
using Brasa.Api.Problems;
using Brasa.Application.Ordering.Management;
using Brasa.Application.Payments;
using Permissions = Brasa.Application.Identity.Permissions;

namespace Brasa.Api.Endpoints.Admin;

internal static class AdminOrderEndpoints
{
    public static RouteGroupBuilder MapAdminOrderEndpoints(this RouteGroupBuilder secured)
    {
        var orders = secured.MapGroup("/orders");

        orders.MapGet("/", async (string[]? status, int? limit, string? cursor, ListOrdersHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(status, limit, cursor, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.OrdersRead)
            .WithName("AdminListOrders");

        orders.MapGet("/{id:guid}", async (Guid id, GetOrderHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.OrdersRead)
            .WithName("AdminGetOrder");

        MapAction(orders, "accept", OrderAction.Accept);
        MapAction(orders, "reject", OrderAction.Reject);
        MapAction(orders, "cancel", OrderAction.Cancel);
        MapAction(orders, "complete", OrderAction.Complete);

        orders.MapPost("/{id:guid}/payment/receive", async (Guid id, ReceivePaymentHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.OrdersManage)
            .WithName("AdminReceivePayment");

        return secured;
    }

    private static void MapAction(RouteGroupBuilder orders, string route, OrderAction action) =>
        orders.MapPost($"/{{id:guid}}/{route}", async (Guid id, ReasonRequest? request, ChangeOrderStatusHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, action, request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.OrdersManage)
            .WithName($"AdminOrder{action}");
}
