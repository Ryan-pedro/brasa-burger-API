using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Brasa.Application.Catalog.Categories;
using Brasa.Application.Catalog.Menu;
using Brasa.Application.Catalog.OptionGroups;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class OptionGroupTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Gerente_cria_grupo_associa_ao_produto_e_o_cardapio_mostra_as_opcoes()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var extras = await manager.OptionGroupAsync("Addon", 0, 2, ("Bacon", 500), ("Queijo", 300));

        var product = await manager.ProductAsync(2490, extras.Id);

        Assert.Equal([extras.Id], product.OptionGroupIds);
        var menuProduct = await MenuProductAsync(product.Id);
        var group = Assert.Single(menuProduct.OptionGroups);
        Assert.Equal("Addon", group.Kind);
        Assert.Equal([500L, 300L], group.Options.Select(option => option.PriceDeltaInCents));
    }

    [Fact]
    public async Task Editar_o_grupo_mantem_o_id_das_opcoes_existentes()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var group = await manager.OptionGroupAsync("Choice", 1, 1, ("Coca", 0), ("Guaraná", 0));
        var coca = group.Options[0];

        var response = await manager.PutAsJsonAsync($"/api/v1/admin/catalog/option-groups/{group.Id}", new
        {
            name = group.Name,
            options = new object[] { new { id = coca.Id, name = "Coca-Cola", priceDeltaInCents = 0 }, new { name = "Suco", priceDeltaInCents = 390 } },
        });

        var updated = await response.ReadAsync<OptionGroupResponse>();
        Assert.Equal(coca.Id, updated.Options[0].Id);
        Assert.Equal("Coca-Cola", updated.Options[0].Name);
        Assert.Equal(["Coca-Cola", "Suco"], updated.Options.Select(option => option.Name));
    }

    [Fact]
    public async Task Grupo_invalido_responde_400_com_os_campos()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);

        var response = await manager.PostAsJsonAsync("/api/v1/admin/catalog/option-groups", new
        {
            name = "Retirar",
            kind = "Removal",
            minSelections = 1,
            options = new[] { new { name = "Cebola", priceDeltaInCents = 100 } },
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = body.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("minSelections", out _));
        Assert.True(errors.TryGetProperty("options[0].priceDeltaInCents", out _));
    }

    [Fact]
    public async Task Escolha_obrigatoria_sem_opcao_disponivel_deixa_o_produto_indisponivel()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var drink = await manager.OptionGroupAsync("Choice", 1, 1, ("Única bebida", 0));
        var product = await manager.ProductAsync(3290, drink.Id);

        var response = await manager.PutAsJsonAsync(
            $"/api/v1/admin/catalog/option-groups/{drink.Id}/options/{drink.Options[0].Id}/availability",
            new { isAvailable = false });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False((await MenuProductAsync(product.Id)).IsAvailable);
    }

    [Fact]
    public async Task Subcategoria_aparece_no_cardapio_com_a_categoria_principal()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);
        var parent = await (await manager.PostAsJsonAsync("/api/v1/admin/catalog/categories", new { name = TestApi.Unique("Bebidas") })).ReadAsync<CategoryResponse>();
        var product = await manager.ProductAsync(600);

        var moved = await (await manager.PutAsJsonAsync(
                $"/api/v1/admin/catalog/categories/{product.CategoryId}/parent",
                new { parentId = parent.Id }))
            .ReadAsync<CategoryResponse>();
        var nested = await manager.PutAsJsonAsync($"/api/v1/admin/catalog/categories/{parent.Id}/parent", new { parentId = moved.Id });

        Assert.Equal(parent.Id, moved.ParentId);
        Assert.Equal(HttpStatusCode.BadRequest, nested.StatusCode);
        var menu = await factory.CreateClient().GetFromJsonAsync<MenuResponse>("/api/v1/store/menu");
        Assert.Contains(menu!.Categories, category => category.Id == moved.Id && category.ParentId == parent.Id);
    }

    [Fact]
    public async Task A_cozinha_nao_acessa_os_grupos_de_opcoes()
    {
        var kitchen = await factory.SignedInAsync(StoreApiFactory.Kitchen);

        var response = await kitchen.GetAsync("/api/v1/admin/catalog/option-groups");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Cardapio_responde_304_quando_nada_mudou()
    {
        var client = factory.CreateClient();
        var first = await client.GetAsync("/api/v1/store/menu");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/store/menu");
        request.Headers.IfNoneMatch.Add(first.Headers.ETag!);
        var second = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
    }

    private async Task<MenuProduct> MenuProductAsync(Guid productId)
    {
        var menu = await factory.CreateClient().GetFromJsonAsync<MenuResponse>("/api/v1/store/menu");
        return menu!.Categories.SelectMany(category => category.Products).Single(product => product.Id == productId);
    }
}
