using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Brasa.Application.Catalog.Categories;
using Brasa.Application.Catalog.Menu;
using Brasa.Application.Catalog.Products;

namespace Brasa.IntegrationTests;

/// <summary>
/// Cardápio: a leitura pública e a edição pelo painel, com as permissões de cada papel.
/// Cada teste cria os próprios dados com nomes únicos, porque o banco é compartilhado.
/// </summary>
[Collection(ApiCollection.Name)]
public class CatalogTests(StoreApiFactory factory)
{
    private const string CsrfHeader = "X-Brasa-Csrf";

    [Fact]
    public async Task Cardapio_publico_mostra_so_categorias_ativas_e_marca_os_indisponiveis()
    {
        var menu = await factory.CreateClient().GetFromJsonAsync<MenuResponse>("/api/v1/store/menu");

        Assert.NotNull(menu);
        Assert.DoesNotContain(menu.Categories, category => category.Name == StoreApiFactory.HiddenCategory);

        var visible = Assert.Single(menu.Categories, category => category.Name == StoreApiFactory.VisibleCategory);
        Assert.Equal("lanches", visible.Slug);

        var available = Assert.Single(visible.Products, product => product.Name == StoreApiFactory.AvailableProduct);
        Assert.Equal(1990, available.PriceInCents);
        Assert.True(available.IsAvailable);
        Assert.True(available.IsFeatured);

        var soldOut = Assert.Single(visible.Products, product => product.Name == StoreApiFactory.UnavailableProduct);
        Assert.False(soldOut.IsAvailable);
    }

    [Fact]
    public async Task Sem_sessao_o_cardapio_do_painel_responde_401()
    {
        var response = await NewClient().GetAsync("/api/v1/admin/catalog/categories");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_cozinha_nao_acessa_o_cardapio_do_painel()
    {
        var client = await SignedInAsync(StoreApiFactory.Kitchen);

        var response = await client.GetAsync("/api/v1/admin/catalog/categories");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("permission_denied", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Gerente_cria_categoria_e_produto_e_eles_aparecem_no_cardapio()
    {
        var client = await SignedInAsync(StoreApiFactory.Manager);

        var categoryResponse = await client.PostAsJsonAsync(
            "/api/v1/admin/catalog/categories",
            new { name = "Porções do Teste" });
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(category);
        Assert.Equal("porcoes-do-teste", category.Slug);

        var productResponse = await client.PostAsJsonAsync(
            "/api/v1/admin/catalog/products",
            new { categoryId = category.Id, name = "Batata do Teste", description = "Crocante.", priceInCents = 1990, size = "Média" });
        Assert.Equal(HttpStatusCode.Created, productResponse.StatusCode);
        var product = await productResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(product);
        Assert.Equal(1990, product.PriceInCents);
        Assert.True(product.IsAvailable);

        var menu = await factory.CreateClient().GetFromJsonAsync<MenuResponse>("/api/v1/store/menu");
        var inMenu = Assert.Single(menu!.Categories, c => c.Id == category.Id);
        Assert.Contains(inMenu.Products, p => p.Id == product.Id && p.Size == "Média");
    }

    [Fact]
    public async Task Produto_com_dados_invalidos_responde_400_com_os_campos()
    {
        var client = await SignedInAsync(StoreApiFactory.Manager);

        var response = await client.PostAsJsonAsync(
            "/api/v1/admin/catalog/products",
            new { categoryId = Guid.NewGuid(), name = "", priceInCents = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("validation_failed", body.RootElement.GetProperty("code").GetString());
        var errors = body.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("name", out _));
        Assert.True(errors.TryGetProperty("priceInCents", out _));
    }

    [Fact]
    public async Task Categoria_com_nome_repetido_responde_409()
    {
        var client = await SignedInAsync(StoreApiFactory.Owner);

        var first = await client.PostAsJsonAsync("/api/v1/admin/catalog/categories", new { name = "Repetida" });
        var second = await client.PostAsJsonAsync("/api/v1/admin/catalog/categories", new { name = "  repetida " });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("category_name_taken", await ReadErrorCodeAsync(second));
    }

    [Fact]
    public async Task Gerente_marca_produto_como_indisponivel()
    {
        var client = await SignedInAsync(StoreApiFactory.Manager);
        var category = await (await client.PostAsJsonAsync("/api/v1/admin/catalog/categories", new { name = "Disponibilidade" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();
        var product = await (await client.PostAsJsonAsync(
                "/api/v1/admin/catalog/products",
                new { categoryId = category!.Id, name = "Vai acabar", priceInCents = 1000 }))
            .Content.ReadFromJsonAsync<ProductResponse>();

        var response = await client.PutAsJsonAsync(
            $"/api/v1/admin/catalog/products/{product!.Id}/availability",
            new { isAvailable = false });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.False(updated!.IsAvailable);
    }

    [Fact]
    public async Task Produto_inexistente_responde_404()
    {
        var client = await SignedInAsync(StoreApiFactory.Manager);

        var response = await client.PutAsJsonAsync(
            $"/api/v1/admin/catalog/products/{Guid.NewGuid()}/availability",
            new { isAvailable = false });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("product_not_found", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Alterar_o_cardapio_sem_o_cabecalho_anti_csrf_e_recusado()
    {
        var client = await SignedInAsync(StoreApiFactory.Owner);
        client.DefaultRequestHeaders.Remove(CsrfHeader);

        var response = await client.PostAsJsonAsync("/api/v1/admin/catalog/categories", new { name = "Sem cabeçalho" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("csrf_validation_failed", await ReadErrorCodeAsync(response));
    }

    private HttpClient NewClient()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CsrfHeader, "1");
        return client;
    }

    private async Task<HttpClient> SignedInAsync(string email)
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync(
            "/api/v1/admin/auth/login",
            new { email, password = StoreApiFactory.Password });
        response.EnsureSuccessStatusCode();
        return client;
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }
}
