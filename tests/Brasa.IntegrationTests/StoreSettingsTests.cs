using System.Net;
using System.Net.Http.Json;
using Brasa.Application.StoreSettings.GetStoreProfile;
using Brasa.Application.StoreSettings.Settings;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class StoreSettingsTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Dono_altera_as_regras_e_o_perfil_publico_reflete()
    {
        await using (await factory.StoreSettingsAsync(new { acceptsDineIn = false, deliveryFeeInCents = 690, minimumOrderInCents = 2000 }))
        {
            var profile = await factory.CreateClient().GetFromJsonAsync<StoreProfileResponse>("/api/v1/store/profile");

            Assert.Equal(["Pickup", "Delivery"], profile!.OrderTypes);
            Assert.Equal(690, profile.DeliveryFeeInCents);
            Assert.Equal(2000, profile.MinimumOrderInCents);
        }
    }

    [Fact]
    public async Task Loja_precisa_aceitar_ao_menos_um_tipo_de_pedido()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);

        var response = await owner.PutAsJsonAsync("/api/v1/admin/store/settings", new { acceptsDineIn = false, acceptsPickup = false, acceptsDelivery = false });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Loja_fechada_recusa_pedidos_com_mensagem_clara()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(1500);

        var closed = await (await manager.PutAsJsonAsync("/api/v1/admin/store/status", new { isOpen = false })).ReadAsync<StoreSettingsResponse>();
        try
        {
            var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/store/orders/quote", new { type = "Pickup", items = new[] { TestData.Item(product.Id) } });

            Assert.False(closed.IsOpen);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("store_closed", await response.ReadErrorCodeAsync());
        }
        finally
        {
            (await manager.PutAsJsonAsync("/api/v1/admin/store/status", new { isOpen = true })).EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task A_cozinha_nao_altera_as_configuracoes_da_loja()
    {
        var kitchen = await factory.SignedInAsync(StoreApiFactory.Kitchen);

        var response = await kitchen.PutAsJsonAsync("/api/v1/admin/store/status", new { isOpen = false });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
