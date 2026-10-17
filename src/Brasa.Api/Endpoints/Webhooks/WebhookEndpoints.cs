using System.Text;
using Brasa.Api.Problems;
using Brasa.Application.Payments.Online;

namespace Brasa.Api.Endpoints.Webhooks;

public static class WebhookEndpoints
{
    public const int MaxBodyBytes = 64 * 1024;

    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder api)
    {
        var webhooks = api.MapGroup("/webhooks").AllowAnonymous();

        webhooks.MapPost("/pix", async (HttpRequest request, ReceivePixWebhookHandler handler, CancellationToken cancellationToken) =>
            {
                if (request.ContentLength > MaxBodyBytes)
                {
                    return ApiProblems.Create(StatusCodes.Status413PayloadTooLarge, "Aviso grande demais.", "webhook_too_large");
                }

                var message = new PixWebhookMessage(await ReadBodyAsync(request, cancellationToken), ReadHeaders(request));
                var receipt = await handler.HandleAsync(message, cancellationToken);

                return receipt == PixWebhookReceipt.Rejected ? Results.Unauthorized() : Results.Ok();
            })
            .WithName("ReceivePixWebhook");

        return api;
    }

    private static async Task<string> ReadBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var buffer = new byte[MaxBodyBytes];
        var length = 0;
        int read;
        while (length < buffer.Length && (read = await request.Body.ReadAsync(buffer.AsMemory(length), cancellationToken)) > 0)
        {
            length += read;
        }

        return Encoding.UTF8.GetString(buffer, 0, length);
    }

    private static Dictionary<string, string> ReadHeaders(HttpRequest request) =>
        request.Headers.ToDictionary(header => header.Key, header => header.Value.ToString(), StringComparer.OrdinalIgnoreCase);
}
