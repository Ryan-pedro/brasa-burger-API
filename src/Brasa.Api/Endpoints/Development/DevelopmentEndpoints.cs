using Brasa.Application.Payments.Online;
using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Infrastructure.Payments;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Api.Endpoints.Development;

public static class DevelopmentEndpoints
{
    public static IEndpointRouteBuilder MapDevelopmentEndpoints(this IEndpointRouteBuilder api)
    {
        var development = api.MapGroup("/dev").AllowAnonymous().ExcludeFromDescription();

        development.MapPost("/pix/orders/{trackingToken}/pay", async (
                string trackingToken,
                SimulatePixPaymentRequest? request,
                IPixGateway gateway,
                IAppDbContext db,
                ReceivePixWebhookHandler handler,
                CancellationToken cancellationToken) =>
            {
                var chargeId = await db.Orders
                    .Where(order => order.TrackingToken == trackingToken)
                    .Join(db.Payments, order => order.Id, payment => payment.OrderId, (_, payment) => payment.ChargeId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (gateway is not FakePixGateway fake || chargeId is null)
                {
                    return Results.NotFound();
                }

                var paidAmount = request?.PaidAmountInCents is { } cents ? Money.FromCents(cents) : (decimal?)null;
                var message = fake.SimulatePayment(chargeId, paidAmount);
                var deliver = request?.DeliverWebhook ?? true;
                if (deliver)
                {
                    await handler.HandleAsync(message, cancellationToken);
                }

                return Results.Ok(new SimulatedPixPaymentResponse(message.Body, message.Headers, deliver));
            })
            .WithName("DevelopmentSimulatePixPayment");

        return api;
    }
}
