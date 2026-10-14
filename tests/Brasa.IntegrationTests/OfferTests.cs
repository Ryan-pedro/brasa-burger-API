using System.Net;
using System.Net.Http.Json;
using Brasa.Application.Catalog.Menu;
using Brasa.Application.Catalog.Offers;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class OfferTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Preco_promocional_aparece_no_cardapio_e_vira_desconto_na_cotacao()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(2990);
        var offer = await (await manager.PostAsJsonAsync("/api/v1/admin/catalog/offers", new
            {
                label = "Smash por 24,90",
                kind = "ProductPrice",
                productId = product.Id,
                promoPriceInCents = 2490,
            }))
            .ReadAsync<OfferResponse>();

        try
        {
            var menu = await factory.CreateClient().GetFromJsonAsync<MenuResponse>("/api/v1/store/menu");
            var quote = await factory.CreateClient().QuoteAsync("Pickup", TestData.Item(product.Id, 2));

            Assert.Equal(2490, menu!.Categories.SelectMany(c => c.Products).Single(p => p.Id == product.Id).PromoPriceInCents);
            Assert.Equal(5980, quote.SubtotalInCents);
            Assert.Equal(1000, quote.Discount!.AmountInCents);
            Assert.Equal(4980, quote.TotalInCents);
        }
        finally
        {
            await DeactivateAsync(manager, offer);
        }
    }

    [Fact]
    public async Task Preco_promocional_maior_que_o_preco_e_recusado()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(1000);

        var response = await manager.PostAsJsonAsync("/api/v1/admin/catalog/offers", new
        {
            label = "Errada",
            kind = "ProductPrice",
            productId = product.Id,
            promoPriceInCents = 1500,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Ofertas_publicas_listam_so_as_vigentes()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var future = await (await manager.PostAsJsonAsync("/api/v1/admin/catalog/offers", new
            {
                label = TestApi.Unique("Futura"),
                kind = "OrderAmount",
                minimumSubtotalInCents = 100000,
                discountInCents = 100,
                startsAt = DateTimeOffset.UtcNow.AddDays(10),
            }))
            .ReadAsync<OfferResponse>();

        var offers = await factory.CreateClient().GetFromJsonAsync<List<OfferResponse>>("/api/v1/store/offers");

        Assert.DoesNotContain(offers!, offer => offer.Id == future.Id);
        await DeactivateAsync(manager, future);
    }

    private static async Task DeactivateAsync(HttpClient manager, OfferResponse offer) =>
        (await manager.PutAsJsonAsync($"/api/v1/admin/catalog/offers/{offer.Id}", offer with { IsActive = false })).EnsureSuccessStatusCode();
}
