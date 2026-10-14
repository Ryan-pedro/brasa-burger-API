using System.Net.Http.Json;
using Brasa.Application.Shared.Outbox;
using Brasa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class RealtimeAndAuditTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Pedido_confirmado_gera_avisos_para_a_cozinha_e_para_o_painel()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1100);
        await (await factory.CreateClient().PlaceOrderAsync(1100, [TestData.Item(product.Id)])).ReadAsync<object>();

        await factory.RunJobAsync<DispatchOutboxJob>();

        Assert.Contains(factory.Notifier.Sent, sent => sent.Channel == RealtimeChannels.Kitchen && sent.Notice.Type == RealtimeTypes.TicketChanged);
        Assert.Contains(factory.Notifier.Sent, sent => sent.Channel == RealtimeChannels.Ops && sent.Notice.Type == RealtimeTypes.OrderChanged);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.OutboxMessages.AnyAsync(message => message.ProcessedAt == null));
    }

    [Fact]
    public async Task Mudanca_de_preco_fica_registrada_na_auditoria()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(1000);

        (await manager.PutAsJsonAsync($"/api/v1/admin/catalog/products/{product.Id}", new
        {
            categoryId = product.CategoryId,
            name = product.Name,
            priceInCents = 1200,
        })).EnsureSuccessStatusCode();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entry = await db.AuditEntries.SingleAsync(e => e.SubjectId == product.Id && e.Action == "product.price_changed");
        Assert.NotNull(entry.UserId);
        Assert.Contains("1200", entry.Data);
    }
}
