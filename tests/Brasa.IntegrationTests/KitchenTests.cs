using System.Net;
using System.Net.Http.Json;
using Brasa.Application.Kitchen.Board;
using Brasa.Application.Kitchen.Tickets;
using Brasa.Application.Ordering.Tracking;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class KitchenTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Pedido_confirmado_aparece_no_quadro_com_as_opcoes_em_linguagem_de_cozinha()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var removal = await manager.OptionGroupAsync("Removal", 0, 2, ("Cebola", 0), ("Tomate", 0));
        var product = await manager.ProductAsync(2290, removal.Id);
        var order = await PlaceAsync(TestData.Item(product.Id, 1, removal.Options[0].Id), 2290);

        var ticket = await TicketOfAsync(order);

        Assert.Equal("Queued", ticket.Preparation);
        Assert.Equal("Cliente Teste", ticket.CustomerLabel);
        Assert.Equal(["SEM CEBOLA"], Assert.Single(ticket.Items).Modifiers);
    }

    [Fact]
    public async Task Avancar_o_preparo_atualiza_o_acompanhamento_do_cliente()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1500);
        var order = await PlaceAsync(TestData.Item(product.Id), 1500);
        var kitchen = await factory.SignedInAsync(StoreApiFactory.Kitchen);
        var ticket = await TicketOfAsync(order);

        var statuses = new List<string>();
        foreach (var from in new[] { "Queued", "InPreparation", "Ready" })
        {
            (await kitchen.PostAsJsonAsync($"/api/v1/kitchen/tickets/{ticket.Id}/advance", new { from })).EnsureSuccessStatusCode();
            statuses.Add((await TrackAsync(order)).Status);
        }

        Assert.Equal(["InPreparation", "Ready", "Completed"], statuses);
        Assert.DoesNotContain((await BoardAsync(kitchen)).Tickets, t => t.Id == ticket.Id);
    }

    [Fact]
    public async Task Desfazer_volta_um_passo_no_preparo_e_no_pedido()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1500);
        var order = await PlaceAsync(TestData.Item(product.Id), 1500);
        var kitchen = await factory.SignedInAsync(StoreApiFactory.Kitchen);
        var ticket = await TicketOfAsync(order);

        (await kitchen.PostAsJsonAsync($"/api/v1/kitchen/tickets/{ticket.Id}/advance", new { from = "Queued" })).EnsureSuccessStatusCode();
        var undone = await (await kitchen.PostAsJsonAsync($"/api/v1/kitchen/tickets/{ticket.Id}/undo", new { from = "InPreparation" })).ReadAsync<TicketResponse>();

        Assert.Equal("Queued", undone.Preparation);
        Assert.Equal("Confirmed", (await TrackAsync(order)).Status);
    }

    [Fact]
    public async Task Dois_toques_simultaneos_avancam_o_preparo_uma_unica_vez()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1500);
        var order = await PlaceAsync(TestData.Item(product.Id), 1500);
        var ticket = await TicketOfAsync(order);
        var first = await factory.SignedInAsync(StoreApiFactory.Kitchen);
        var second = await factory.SignedInAsync(StoreApiFactory.Kitchen);

        var responses = await Task.WhenAll(
            first.PostAsJsonAsync($"/api/v1/kitchen/tickets/{ticket.Id}/advance", new { from = "Queued" }),
            second.PostAsJsonAsync($"/api/v1/kitchen/tickets/{ticket.Id}/advance", new { from = "Queued" }));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        Assert.Equal("InPreparation", (await TrackAsync(order)).Status);
    }

    [Fact]
    public async Task Gerente_sem_papel_de_cozinha_recebe_403_no_quadro()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);

        var response = await manager.GetAsync("/api/v1/kitchen/tickets");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Nenhuma_rota_publica_devolve_comanda()
    {
        var routes = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText ?? string.Empty)
            .Where(route => route.StartsWith("/api/v1/store", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(routes);
        Assert.DoesNotContain(routes, route => route.Contains("ticket", StringComparison.OrdinalIgnoreCase) || route.Contains("kitchen", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Hub_da_cozinha_exige_sessao()
    {
        var response = await factory.CreateClient().PostAsync("/api/v1/hubs/kitchen/negotiate?negotiateVersion=1", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<OrderTrackingResponse> PlaceAsync(object item, long totalInCents) =>
        await (await factory.CreateClient().PlaceOrderAsync(totalInCents, [item])).ReadAsync<OrderTrackingResponse>();

    private async Task<OrderTrackingResponse> TrackAsync(OrderTrackingResponse order) =>
        (await factory.CreateClient().GetFromJsonAsync<OrderTrackingResponse>($"/api/v1/store/orders/{order.TrackingToken}"))!;

    private static async Task<BoardResponse> BoardAsync(HttpClient kitchen) =>
        (await kitchen.GetFromJsonAsync<BoardResponse>("/api/v1/kitchen/tickets"))!;

    private async Task<TicketResponse> TicketOfAsync(OrderTrackingResponse order)
    {
        var kitchen = await factory.SignedInAsync(StoreApiFactory.Kitchen);
        return (await BoardAsync(kitchen)).Tickets.Single(ticket => ticket.OrderNumber == order.Number);
    }
}
