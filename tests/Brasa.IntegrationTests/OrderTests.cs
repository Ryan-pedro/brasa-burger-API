using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Brasa.Application.Ordering.Tracking;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class OrderTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Cotacao_calcula_os_precos_no_servidor_com_as_opcoes()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var extras = await manager.OptionGroupAsync("Addon", 0, 3, ("Bacon", 500), ("Ovo", 250));
        var product = await manager.ProductAsync(2490, extras.Id);

        var quote = await factory.CreateClient().QuoteAsync("Pickup", TestData.Item(product.Id, 2, extras.Options[0].Id));

        var line = Assert.Single(quote.Lines);
        Assert.Equal(2990, line.UnitPriceInCents);
        Assert.Equal(5980, quote.TotalInCents);
        Assert.Equal("Bacon", Assert.Single(line.Options).Name);
    }

    [Fact]
    public async Task Pedido_com_total_adulterado_responde_cart_changed_e_nao_cria_pedido()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1990);
        var key = Guid.NewGuid().ToString();

        var response = await factory.CreateClient().PlaceOrderAsync(1, [TestData.Item(product.Id)], idempotencyKey: key);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("cart_changed", body.RootElement.GetProperty("code").GetString());
        Assert.Equal(1990, body.RootElement.GetProperty("quote").GetProperty("totalInCents").GetInt64());

        var retry = await factory.CreateClient().PlaceOrderAsync(1990, [TestData.Item(product.Id)], idempotencyKey: key);
        Assert.Equal(HttpStatusCode.Created, retry.StatusCode);
    }

    [Fact]
    public async Task O_mesmo_checkout_enviado_tres_vezes_cria_um_unico_pedido()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1200);
        var client = factory.CreateClient();
        var key = Guid.NewGuid().ToString();

        var responses = new List<HttpResponseMessage>();
        for (var attempt = 0; attempt < 3; attempt++)
        {
            responses.Add(await client.PlaceOrderAsync(1200, [TestData.Item(product.Id)], idempotencyKey: key));
        }

        Assert.Equal([HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.OK], responses.Select(response => response.StatusCode));
        var numbers = await Task.WhenAll(responses.Select(async response => (await response.Content.ReadFromJsonAsync<OrderTrackingResponse>())!.Number));
        Assert.Single(numbers.Distinct());
    }

    [Fact]
    public async Task Mesma_chave_com_outro_pedido_responde_422()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1000);
        var client = factory.CreateClient();
        var key = Guid.NewGuid().ToString();

        await client.PlaceOrderAsync(1000, [TestData.Item(product.Id)], idempotencyKey: key);
        var reused = await client.PlaceOrderAsync(2000, [TestData.Item(product.Id, 2)], idempotencyKey: key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, reused.StatusCode);
        Assert.Equal("idempotency_key_reused", await reused.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Vinte_pedidos_simultaneos_recebem_vinte_numeros_distintos()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(900);

        var responses = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ =>
            factory.CreateClient().PlaceOrderAsync(900, [TestData.Item(product.Id)])));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        var numbers = await Task.WhenAll(responses.Select(async response => (await response.Content.ReadFromJsonAsync<OrderTrackingResponse>())!.Number));
        Assert.Equal(20, numbers.Distinct().Count());
    }

    [Fact]
    public async Task Mudar_o_preco_depois_da_compra_nao_altera_o_pedido()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var product = await manager.ProductAsync(1500);
        var order = await (await factory.CreateClient().PlaceOrderAsync(1500, [TestData.Item(product.Id)])).ReadAsync<OrderTrackingResponse>();

        (await manager.PutAsJsonAsync($"/api/v1/admin/catalog/products/{product.Id}", new
        {
            categoryId = product.CategoryId,
            name = product.Name,
            priceInCents = 9900,
        })).EnsureSuccessStatusCode();

        var tracked = await factory.CreateClient().GetFromJsonAsync<OrderTrackingResponse>($"/api/v1/store/orders/{order.TrackingToken}");
        Assert.Equal(1500, tracked!.TotalInCents);
        Assert.Equal(1500, Assert.Single(tracked.Items).TotalInCents);
    }

    [Fact]
    public async Task Item_indisponivel_responde_409_indicando_o_item()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/store/orders/quote", new
        {
            type = "Pickup",
            items = new[] { TestData.Item(await ProductIdAsync(StoreApiFactory.UnavailableProduct)) },
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("item_unavailable", body.RootElement.GetProperty("code").GetString());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("items[0]", out _));
    }

    [Fact]
    public async Task Escolha_obrigatoria_nao_atendida_responde_400()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var drink = await manager.OptionGroupAsync("Choice", 1, 1, ("Coca", 0), ("Guaraná", 0));
        var combo = await manager.ProductAsync(3290, drink.Id);

        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/store/orders/quote", new { type = "Pickup", items = new[] { TestData.Item(combo.Id) } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delivery_sem_endereco_responde_400_com_os_campos()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1000);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/store/orders")
        {
            Content = JsonContent.Create(new
            {
                type = "Delivery",
                items = new[] { TestData.Item(product.Id) },
                customer = new { name = "Ana", phone = "31999990000" },
                paymentMethod = "Cash",
                expectedTotalInCents = 1000,
            }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("address.street", out _));
    }

    [Fact]
    public async Task Pedido_sem_chave_de_idempotencia_responde_400()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/store/orders", new { type = "Pickup" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("idempotencyKey", out _));
    }

    [Fact]
    public async Task Pedido_abaixo_do_minimo_responde_422()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(500);

        await using (await factory.StoreSettingsAsync(new { minimumOrderInCents = 1000 }))
        {
            var response = await factory.CreateClient().PlaceOrderAsync(500, [TestData.Item(product.Id)]);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal("below_minimum_order", await response.ReadErrorCodeAsync());
        }
    }

    [Fact]
    public async Task Pedido_e_acompanhado_so_pelo_token()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1800);
        var created = await (await factory.CreateClient().PlaceOrderAsync(1800, [TestData.Item(product.Id)], type: "Delivery")).ReadAsync<OrderTrackingResponse>();

        var byToken = await factory.CreateClient().GetAsync($"/api/v1/store/orders/{created.TrackingToken}");
        var byNumber = await factory.CreateClient().GetAsync($"/api/v1/store/orders/{created.Number}");

        Assert.Equal(HttpStatusCode.OK, byToken.StatusCode);
        Assert.Equal("Confirmed", created.Status);
        Assert.Equal(HttpStatusCode.NotFound, byNumber.StatusCode);
        Assert.Equal(32, created.TrackingToken.Length);
    }

    private async Task<Guid> ProductIdAsync(string name)
    {
        var menu = await factory.CreateClient().GetFromJsonAsync<JsonElement>("/api/v1/store/menu");
        return menu.GetProperty("categories").EnumerateArray()
            .SelectMany(category => category.GetProperty("products").EnumerateArray())
            .Single(product => product.GetProperty("name").GetString() == name)
            .GetProperty("id").GetGuid();
    }
}
