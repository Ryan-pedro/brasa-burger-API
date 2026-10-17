using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class RateLimitTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Login_acima_do_limite_responde_429_com_tempo_de_espera()
    {
        await using var limited = WithLimit("RateLimits:LoginPerMinute", 2);
        var client = limited.CreateClient();
        client.DefaultRequestHeaders.Add(TestApi.CsrfHeader, "1");

        await client.LoginAsync("ninguem@teste.dev", "senha-errada-1");
        await client.LoginAsync("ninguem@teste.dev", "senha-errada-2");
        var blocked = await client.LoginAsync("ninguem@teste.dev");

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Equal("rate_limited", await blocked.ReadErrorCodeAsync());
        Assert.True(blocked.Headers.RetryAfter?.Delta > TimeSpan.Zero);
    }

    [Fact]
    public async Task Limite_geral_vale_para_toda_a_api_menos_a_verificacao_de_saude()
    {
        await using var limited = WithLimit("RateLimits:GlobalPerMinute", 3);
        var client = limited.CreateClient();

        for (var request = 0; request < 3; request++)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/store/profile")).StatusCode);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.GetAsync("/api/v1/store/menu")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/health/ready")).StatusCode);
    }

    [Fact]
    public async Task Consulta_de_pedido_pelo_token_tem_limite_proprio()
    {
        await using var limited = WithLimit("RateLimits:OrderLookupsPerMinute", 1);
        var client = limited.CreateClient();
        var token = new string('0', 32);

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/store/orders/{token}")).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.GetAsync($"/api/v1/store/orders/{token}/payment")).StatusCode);
    }

    private WebApplicationFactory<Brasa.Api.AssemblyReference> WithLimit(string setting, int permitsPerMinute) =>
        factory.WithWebHostBuilder(builder => builder.UseSetting(setting, permitsPerMinute.ToString()));
}
