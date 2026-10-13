using System.Net;
using System.Net.Http.Json;
using Brasa.Application.StoreSettings.GetStoreProfile;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class StoreProfileTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Perfil_publico_devolve_os_dados_da_loja()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/store/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<StoreProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal(StoreApiFactory.StoreName, profile.Name);
        Assert.Equal("America/Sao_Paulo", profile.TimeZoneId);
    }

    [Fact]
    public async Task Perfil_publico_nao_exige_login()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/store/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }
}
