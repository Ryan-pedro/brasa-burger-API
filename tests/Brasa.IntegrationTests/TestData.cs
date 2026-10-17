using System.Net.Http.Json;
using Brasa.Application.Catalog.Categories;
using Brasa.Application.Catalog.OptionGroups;
using Brasa.Application.Catalog.Products;
using Brasa.Application.Identity.Team;
using Brasa.Application.Ordering.Checkout;
using Brasa.Application.StoreSettings.Settings;

namespace Brasa.IntegrationTests;

public static class TestData
{
    public static async Task<ProductResponse> ProductAsync(this HttpClient manager, long priceInCents, params Guid[] optionGroupIds)
    {
        var category = await (await manager.PostAsJsonAsync(
                "/api/v1/admin/catalog/categories",
                new { name = TestApi.Unique("Categoria") }))
            .ReadAsync<CategoryResponse>();

        return await (await manager.PostAsJsonAsync(
                "/api/v1/admin/catalog/products",
                new { categoryId = category.Id, name = TestApi.Unique("Produto"), priceInCents, optionGroupIds }))
            .ReadAsync<ProductResponse>();
    }

    public static async Task<OptionGroupResponse> OptionGroupAsync(this HttpClient manager, string kind, int min, int max, params (string Name, long PriceInCents)[] options) =>
        await (await manager.PostAsJsonAsync(
                "/api/v1/admin/catalog/option-groups",
                new
                {
                    name = TestApi.Unique("Grupo"),
                    kind,
                    minSelections = min,
                    maxSelections = max,
                    options = options.Select(option => new { name = option.Name, priceDeltaInCents = option.PriceInCents }),
                }))
            .ReadAsync<OptionGroupResponse>();

    public static object Item(Guid productId, int quantity = 1, params Guid[] optionIds) =>
        new { productId, quantity, optionIds };

    public static async Task<QuoteResponse> QuoteAsync(this HttpClient client, string type, params object[] items) =>
        await (await client.PostAsJsonAsync("/api/v1/store/orders/quote", new { type, items })).ReadAsync<QuoteResponse>();

    public static Task<HttpResponseMessage> PlaceOrderAsync(
        this HttpClient client,
        long expectedTotalInCents,
        object[] items,
        string type = "Pickup",
        string? idempotencyKey = null,
        string paymentMethod = "Pix") =>
        client.SendAsync(OrderRequest(expectedTotalInCents, items, type, idempotencyKey ?? Guid.NewGuid().ToString(), paymentMethod));

    public static HttpRequestMessage OrderRequest(long expectedTotalInCents, object[] items, string type, string idempotencyKey, string paymentMethod = "Pix")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/store/orders")
        {
            Content = JsonContent.Create(new
            {
                type,
                items,
                customer = new { name = "Cliente Teste", phone = "(31) 99999-0000", table = "7" },
                address = type == "Delivery" ? new { street = "Rua A", number = "10", district = "Centro" } : null,
                paymentMethod,
                expectedTotalInCents,
            }),
        };
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        return request;
    }

    public static async Task<CreatedInvitationResponse> InviteAsync(this HttpClient owner, string email, string role) =>
        await (await owner.PostAsJsonAsync("/api/v1/admin/team/invitations", new { email, displayName = "Pessoa da equipe", role }))
            .ReadAsync<CreatedInvitationResponse>();

    public static async Task<IAsyncDisposable> StoreSettingsAsync(this StoreApiFactory factory, object changes)
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var original = await (await owner.GetAsync("/api/v1/admin/store/settings")).ReadAsync<StoreSettingsResponse>();

        (await owner.PutAsJsonAsync("/api/v1/admin/store/settings", changes)).EnsureSuccessStatusCode();

        return new Restore(owner, original);
    }

    private sealed class Restore(HttpClient owner, StoreSettingsResponse original) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            (await owner.PutAsJsonAsync("/api/v1/admin/store/settings", original)).EnsureSuccessStatusCode();
            (await owner.PutAsJsonAsync("/api/v1/admin/store/status", new { original.IsOpen })).EnsureSuccessStatusCode();
            owner.Dispose();
        }
    }
}
