using System.Net;
using System.Net.Http.Json;
using Brasa.Application.Ordering.Management;
using Brasa.Application.Ordering.Tracking;
using Brasa.Application.Privacy;
using Brasa.Domain.Ordering;
using Brasa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class PrivacyTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Pedido_encerrado_ha_mais_de_doze_meses_perde_os_dados_pessoais_e_mantem_os_valores()
    {
        await TestPrintAgent.InstallAsync(factory);
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(2100);
        var old = await PlaceDeliveryAsync(product.Id, 2100);
        var oldDetail = await CancelAsync(manager, old);
        Assert.True(await PrintJobsForTicketAsync(oldDetail.Ticket!.Id) > 0);

        factory.Clock.Advance(PersonalDataRetention.Period + TimeSpan.FromDays(1));
        manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var recent = await PlaceDeliveryAsync(product.Id, 2100);
        await CancelAsync(manager, recent);

        await factory.RunJobAsync<RemoveExpiredPersonalDataJob>();

        var anonymized = await (await manager.GetAsync($"/api/v1/admin/orders/{oldDetail.Id}")).ReadAsync<OrderDetailResponse>();
        Assert.Equal(OrderCustomer.AnonymizedName, anonymized.Customer.Name);
        Assert.Null(anonymized.Customer.Phone);
        Assert.Null(anonymized.Address);
        Assert.Null(anonymized.Notes);
        Assert.Equal(oldDetail.TotalInCents, anonymized.TotalInCents);
        Assert.Equal(0, await PrintJobsForTicketAsync(oldDetail.Ticket.Id));

        var untouched = await (await manager.GetAsync($"/api/v1/admin/orders/{(await DetailAsync(manager, recent)).Id}")).ReadAsync<OrderDetailResponse>();
        Assert.Equal("Cliente Teste", untouched.Customer.Name);
        Assert.NotNull(untouched.Address);
    }

    [Fact]
    public async Task Pedido_do_titular_remove_os_dados_dos_pedidos_encerrados_pelo_telefone()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(1500);
        var finished = await CancelAsync(manager, await PlaceDeliveryAsync(product.Id, 1500));
        var open = await PlaceDeliveryAsync(product.Id, 1500);

        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var result = await (await owner.PostAsJsonAsync("/api/v1/admin/privacy/personal-data-removals", new { phone = "31 99999 0000" }))
            .ReadAsync<PersonalDataRemovalResponse>();

        Assert.True(result.RemovedOrders >= 1);
        Assert.Contains(open.Number, result.UnfinishedOrderNumbers);
        var detail = await (await manager.GetAsync($"/api/v1/admin/orders/{finished.Id}")).ReadAsync<OrderDetailResponse>();
        Assert.Equal(OrderCustomer.AnonymizedName, detail.Customer.Name);
        Assert.Null(detail.Customer.Phone);
    }

    [Fact]
    public async Task Pedido_do_titular_exige_dado_de_busca_e_e_exclusivo_do_proprietario()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var empty = await owner.PostAsJsonAsync("/api/v1/admin/privacy/personal-data-removals", new { phone = "12" });
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);

        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var forbidden = await manager.PostAsJsonAsync("/api/v1/admin/privacy/personal-data-removals", new { orderNumbers = new[] { 1 } });
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    private async Task<OrderTrackingResponse> PlaceDeliveryAsync(Guid productId, long totalInCents)
    {
        var response = await factory.CreateClient().PlaceOrderAsync(totalInCents + await DeliveryFeeAsync(), [TestData.Item(productId)], type: "Delivery");
        return await response.ReadAsync<OrderTrackingResponse>();
    }

    private async Task<long> DeliveryFeeAsync()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var settings = await (await owner.GetAsync("/api/v1/admin/store/settings")).ReadAsync<Brasa.Application.StoreSettings.Settings.StoreSettingsResponse>();
        return settings.DeliveryFeeInCents;
    }

    private static async Task<OrderDetailResponse> DetailAsync(HttpClient manager, OrderTrackingResponse order)
    {
        var page = await (await manager.GetAsync("/api/v1/admin/orders?limit=100")).ReadAsync<OrdersPage>();
        var id = page.Items.Single(summary => summary.Number == order.Number).Id;
        return await (await manager.GetAsync($"/api/v1/admin/orders/{id}")).ReadAsync<OrderDetailResponse>();
    }

    private static async Task<OrderDetailResponse> CancelAsync(HttpClient manager, OrderTrackingResponse order)
    {
        var detail = await DetailAsync(manager, order);
        return await (await manager.PostAsJsonAsync($"/api/v1/admin/orders/{detail.Id}/cancel", new { reason = "Teste de privacidade" })).ReadAsync<OrderDetailResponse>();
    }

    private async Task<int> PrintJobsForTicketAsync(Guid ticketId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().PrintJobs.CountAsync(job => job.TicketId == ticketId);
    }
}
