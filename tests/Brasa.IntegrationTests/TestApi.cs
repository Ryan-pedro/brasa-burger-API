using System.Net.Http.Json;
using System.Text.Json;
using Brasa.Application.Shared.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.IntegrationTests;

public static class TestApi
{
    public const string CsrfHeader = "X-Brasa-Csrf";

    public static HttpClient NewClient(this StoreApiFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CsrfHeader, "1");
        return client;
    }

    public static Task<HttpResponseMessage> LoginAsync(this HttpClient client, string email, string password = StoreApiFactory.Password) =>
        client.PostAsJsonAsync("/api/v1/admin/auth/login", new { email, password });

    public static async Task<HttpClient> SignedInAsync(this StoreApiFactory factory, string email)
    {
        var client = factory.NewClient();
        (await client.LoginAsync(email)).EnsureSuccessStatusCode();
        return client;
    }

    public static async Task<string?> ReadErrorCodeAsync(this HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public static async Task RunJobAsync<TJob>(this StoreApiFactory factory)
        where TJob : IJob
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TJob>().RunAsync(CancellationToken.None);
    }

    public static string Unique(string prefix) => $"{prefix} {Guid.NewGuid().ToString("N")[..8]}";
}
