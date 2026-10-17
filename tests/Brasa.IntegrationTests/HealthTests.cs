using System.Net;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class HealthTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Prontidao_confere_o_banco()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
