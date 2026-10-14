using System.Net;
using System.Net.Http.Json;
using Brasa.Application.Kitchen.Board;
using Brasa.Application.Ordering.Management;
using Brasa.Application.Ordering.Tracking;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class AdminOrderTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Com_aceite_manual_o_pedido_so_vai_para_a_cozinha_depois_de_aceito()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(2100);

        await using (await factory.StoreSettingsAsync(new { manualAcceptance = true }))
        {
            var order = await PlaceAsync(product.Id, 2100);
            var detail = await DetailAsync(manager, order);

            Assert.Equal("Placed", detail.Status);
            Assert.Null(detail.Ticket);
            Assert.Contains("accept", detail.Actions);

            var accepted = await (await manager.PostAsync($"/api/v1/admin/orders/{detail.Id}/accept", null)).ReadAsync<OrderDetailResponse>();

            Assert.Equal("Confirmed", accepted.Status);
            Assert.Equal("Queued", accepted.Ticket!.Preparation);
            Assert.Contains(await BoardAsync(), ticket => ticket == order.Number);
        }
    }

    [Fact]
    public async Task Pedido_sem_resposta_no_prazo_e_recusado_automaticamente()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(1300);

        await using (await factory.StoreSettingsAsync(new { manualAcceptance = true, acceptanceTimeoutMinutes = 5 }))
        {
            var order = await PlaceAsync(product.Id, 1300);

            factory.Clock.Advance(TimeSpan.FromMinutes(6));
            await factory.RunJobAsync<RejectStaleOrdersJob>();

            var tracked = await factory.CreateClient().GetFromJsonAsync<OrderTrackingResponse>($"/api/v1/store/orders/{order.TrackingToken}");
            Assert.Equal("Rejected", tracked!.Status);
            Assert.Equal(RejectStaleOrdersJob.Reason, tracked.StatusReason);
        }
    }

    [Fact]
    public async Task Cancelar_exige_motivo_e_anula_a_comanda()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(1700);
        var order = await PlaceAsync(product.Id, 1700);
        var detail = await DetailAsync(manager, order);

        var withoutReason = await manager.PostAsJsonAsync($"/api/v1/admin/orders/{detail.Id}/cancel", new { reason = "" });
        var cancelled = await (await manager.PostAsJsonAsync($"/api/v1/admin/orders/{detail.Id}/cancel", new { reason = "Cliente desistiu" })).ReadAsync<OrderDetailResponse>();
        var again = await manager.PostAsJsonAsync($"/api/v1/admin/orders/{detail.Id}/cancel", new { reason = "De novo" });

        Assert.Equal(HttpStatusCode.BadRequest, withoutReason.StatusCode);
        Assert.Equal("Cancelled", cancelled.Status);
        Assert.Equal("Voided", cancelled.Ticket!.Document);
        Assert.DoesNotContain(await BoardAsync(), number => number == order.Number);
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal("invalid_transition", await again.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Gerente_registra_o_pagamento_recebido()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(2500);
        var order = await PlaceAsync(product.Id, 2500);
        var detail = await DetailAsync(manager, order);

        var paid = await (await manager.PostAsync($"/api/v1/admin/orders/{detail.Id}/payment/receive", null)).ReadAsync<OrderDetailResponse>();
        var twice = await manager.PostAsync($"/api/v1/admin/orders/{detail.Id}/payment/receive", null);

        Assert.Equal("Pending", detail.Payment.Status);
        Assert.Equal("Paid", paid.Payment.Status);
        Assert.NotNull(paid.Payment.PaidAt);
        Assert.Equal(HttpStatusCode.Conflict, twice.StatusCode);
    }

    [Fact]
    public async Task Lista_de_pedidos_e_paginada_por_cursor()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(800);
        for (var count = 0; count < 3; count++)
        {
            await PlaceAsync(product.Id, 800);
        }

        var first = await manager.GetFromJsonAsync<OrdersPage>("/api/v1/admin/orders?limit=2&status=Confirmed");
        var second = await manager.GetFromJsonAsync<OrdersPage>($"/api/v1/admin/orders?limit=2&status=Confirmed&cursor={first!.NextCursor}");

        Assert.Equal(2, first.Items.Count);
        Assert.NotNull(first.NextCursor);
        Assert.True(second!.Items[0].Number < first.Items[^1].Number);
        Assert.All(first.Items.Concat(second.Items), order => Assert.Equal("Confirmed", order.Status));
    }

    [Fact]
    public async Task A_cozinha_nao_acessa_o_painel_de_pedidos()
    {
        var kitchen = await factory.SignedInAsync(StoreApiFactory.Kitchen);

        var response = await kitchen.GetAsync("/api/v1/admin/orders");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<OrderTrackingResponse> PlaceAsync(Guid productId, long totalInCents) =>
        await (await factory.CreateClient().PlaceOrderAsync(totalInCents, [TestData.Item(productId)])).ReadAsync<OrderTrackingResponse>();

    private static async Task<OrderDetailResponse> DetailAsync(HttpClient manager, OrderTrackingResponse order)
    {
        var page = await manager.GetFromJsonAsync<OrdersPage>("/api/v1/admin/orders?limit=100");
        var id = page!.Items.Single(item => item.Number == order.Number).Id;
        return (await manager.GetFromJsonAsync<OrderDetailResponse>($"/api/v1/admin/orders/{id}"))!;
    }

    private async Task<IReadOnlyList<int>> BoardAsync()
    {
        var kitchen = await factory.SignedInAsync(StoreApiFactory.Kitchen);
        var board = await kitchen.GetFromJsonAsync<BoardResponse>("/api/v1/kitchen/tickets");
        return board!.Tickets.Select(ticket => ticket.OrderNumber).ToList();
    }
}
