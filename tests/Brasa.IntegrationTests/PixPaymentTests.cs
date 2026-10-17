using System.Net;
using System.Net.Http.Json;
using Brasa.Api.Endpoints.Development;
using Brasa.Application.Ordering.Management;
using Brasa.Application.Ordering.Tracking;
using Brasa.Application.Payments.Online;
using Brasa.Application.StoreSettings.GetStoreProfile;
using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;
using Brasa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class PixPaymentTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Pix_online_so_e_aceito_quando_a_loja_ativa()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1800);

        var refused = await factory.CreateClient().PlaceOrderAsync(1800, [TestData.Item(product.Id)], paymentMethod: "PixOnline");
        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Contains("paymentMethod", await refused.Content.ReadAsStringAsync());

        await using (await factory.StoreSettingsAsync(new { acceptsOnlinePix = true }))
        {
            var profile = await factory.CreateClient().GetFromJsonAsync<StoreProfileResponse>("/api/v1/store/profile");
            Assert.True(profile!.AcceptsOnlinePix);
            Assert.Equal(HttpStatusCode.Created, (await factory.CreateClient().PlaceOrderAsync(1800, [TestData.Item(product.Id)], paymentMethod: "PixOnline")).StatusCode);
        }
    }

    [Fact]
    public async Task Pix_pago_libera_o_pedido_para_a_cozinha()
    {
        await using var _ = await factory.StoreSettingsAsync(new { acceptsOnlinePix = true });
        var order = await PlacePixOrderAsync(2500);
        Assert.Equal("AwaitingPayment", order.Status);

        var payment = await PixPaymentAsync(order);
        Assert.Equal("Pending", payment.Status);
        Assert.NotNull(payment.PixCopyPaste);
        Assert.Equal(payment.PixCopyPaste, (await PixPaymentAsync(order)).PixCopyPaste);

        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var waiting = await DetailAsync(manager, order);
        Assert.Null(waiting.Ticket);
        Assert.DoesNotContain(OrderActions.ReceivePayment, waiting.Actions);

        await PayAsync(order);
        await factory.RunJobAsync<ProcessPaymentWebhooksJob>();

        var paid = await DetailAsync(manager, order);
        Assert.Equal("Confirmed", paid.Status);
        Assert.Equal("Paid", paid.Payment.Status);
        Assert.NotNull(paid.Ticket);
        Assert.Equal("Paid", (await PixPaymentAsync(order)).Status);
    }

    [Fact]
    public async Task Aviso_com_assinatura_invalida_e_recusado_e_aviso_repetido_nao_tem_efeito()
    {
        await using var _ = await factory.StoreSettingsAsync(new { acceptsOnlinePix = true });
        var order = await PlacePixOrderAsync(1300);
        await PixPaymentAsync(order);
        var simulated = await PayAsync(order, deliverWebhook: false);

        var forged = new HttpRequestMessage(HttpMethod.Post, "/api/v1/webhooks/pix") { Content = new StringContent(simulated.Body) };
        forged.Headers.Add("X-Fake-Pix-Signature", new string('0', 64));
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().SendAsync(forged)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await DeliverAsync(simulated)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await DeliverAsync(simulated)).StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var chargeId = await ChargeIdAsync(order);
        Assert.Equal(1, await db.PaymentWebhookEvents.CountAsync(e => e.ChargeId == chargeId));
    }

    [Fact]
    public async Task Pix_nao_pago_expira_sem_chegar_a_cozinha_e_pagamento_tardio_gera_alerta()
    {
        await using var _ = await factory.StoreSettingsAsync(new { acceptsOnlinePix = true });
        var order = await PlacePixOrderAsync(1600);
        await PixPaymentAsync(order);

        factory.Clock.Advance(PixPaymentPolicy.PaymentWindow);
        await factory.RunJobAsync<ReconcilePixPaymentsJob>();

        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var expired = await DetailAsync(manager, order);
        Assert.Equal("PaymentExpired", expired.Status);
        Assert.Equal(Order.PaymentExpiredReason, expired.StatusReason);
        Assert.Null(expired.Ticket);

        await PayAsync(order);
        await factory.RunJobAsync<ProcessPaymentWebhooksJob>();

        var late = await DetailAsync(manager, order);
        Assert.Equal("PaymentExpired", late.Status);
        Assert.Equal("PaidLate", late.Payment.Status);
        Assert.True(await AuditCountAsync("payment.alert.PaidAfterExpiration") > 0);
    }

    [Fact]
    public async Task Valor_divergente_gera_alerta_e_nao_confirma_o_pedido()
    {
        await using var _ = await factory.StoreSettingsAsync(new { acceptsOnlinePix = true });
        var order = await PlacePixOrderAsync(2000);
        await PixPaymentAsync(order);

        await PayAsync(order, paidAmountInCents: 100);
        await factory.RunJobAsync<ProcessPaymentWebhooksJob>();

        var detail = await DetailAsync(await factory.SignedInAsync(StoreApiFactory.Manager), order);
        Assert.Equal("PaymentExpired", detail.Status);
        Assert.Equal(Order.PaymentMismatchReason, detail.StatusReason);
        Assert.Equal("AmountMismatch", detail.Payment.Status);
        Assert.Null(detail.Ticket);
        Assert.True(await AuditCountAsync("payment.alert.AmountMismatch") > 0);
    }

    [Fact]
    public async Task Sem_o_aviso_a_conferencia_periodica_confirma_o_pagamento()
    {
        await using var _ = await factory.StoreSettingsAsync(new { acceptsOnlinePix = true });
        var order = await PlacePixOrderAsync(1700);
        await PixPaymentAsync(order);

        await PayAsync(order, deliverWebhook: false);
        factory.Clock.Advance(PixPaymentPolicy.ReconcileAfter);
        await factory.RunJobAsync<ReconcilePixPaymentsJob>();

        var detail = await DetailAsync(await factory.SignedInAsync(StoreApiFactory.Manager), order);
        Assert.Equal("Confirmed", detail.Status);
        Assert.Equal("Paid", detail.Payment.Status);
    }

    [Fact]
    public async Task Pedido_sem_pix_online_nao_tem_pagamento_pix()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(900);
        var order = await (await factory.CreateClient().PlaceOrderAsync(900, [TestData.Item(product.Id)])).ReadAsync<OrderTrackingResponse>();

        var response = await factory.CreateClient().GetAsync($"/api/v1/store/orders/{order.TrackingToken}/payment");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("pix_payment_not_found", await response.ReadErrorCodeAsync());
    }

    private async Task<OrderTrackingResponse> PlacePixOrderAsync(long priceInCents)
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(priceInCents);
        return await (await factory.CreateClient().PlaceOrderAsync(priceInCents, [TestData.Item(product.Id)], paymentMethod: "PixOnline")).ReadAsync<OrderTrackingResponse>();
    }

    private async Task<PixPaymentResponse> PixPaymentAsync(OrderTrackingResponse order) =>
        await (await factory.CreateClient().GetAsync($"/api/v1/store/orders/{order.TrackingToken}/payment")).ReadAsync<PixPaymentResponse>();

    private async Task<string> ChargeIdAsync(OrderTrackingResponse order)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var orderId = await db.Orders.Where(o => o.TrackingToken == order.TrackingToken).Select(o => o.Id).SingleAsync();
        return (await db.Payments.SingleAsync(p => p.OrderId == orderId)).ChargeId!;
    }

    private async Task<SimulatedPixPaymentResponse> PayAsync(OrderTrackingResponse order, long? paidAmountInCents = null, bool deliverWebhook = true) =>
        await (await factory.CreateClient().PostAsJsonAsync($"/api/v1/dev/pix/orders/{order.TrackingToken}/pay", new { paidAmountInCents, deliverWebhook }))
            .ReadAsync<SimulatedPixPaymentResponse>();

    private Task<HttpResponseMessage> DeliverAsync(SimulatedPixPaymentResponse simulated)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/webhooks/pix") { Content = new StringContent(simulated.Body) };
        foreach (var (name, value) in simulated.Headers)
        {
            request.Headers.Add(name, value);
        }

        return factory.CreateClient().SendAsync(request);
    }

    private static async Task<OrderDetailResponse> DetailAsync(HttpClient manager, OrderTrackingResponse order)
    {
        var page = await (await manager.GetAsync("/api/v1/admin/orders?limit=100")).ReadAsync<OrdersPage>();
        var id = page.Items.Single(summary => summary.Number == order.Number).Id;
        return await (await manager.GetAsync($"/api/v1/admin/orders/{id}")).ReadAsync<OrderDetailResponse>();
    }

    private async Task<int> AuditCountAsync(string action)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().AuditEntries.CountAsync(entry => entry.Action == action);
    }
}
