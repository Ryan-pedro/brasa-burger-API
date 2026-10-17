using Brasa.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class DataProtectionKeyTests(StoreApiFactory factory)
{
    [Fact]
    public void Chaves_que_protegem_sessoes_e_tokens_ficam_no_banco()
    {
        var options = factory.Services.GetRequiredService<IOptions<KeyManagementOptions>>().Value;

        Assert.IsType<EntityFrameworkCoreXmlRepository<AppDbContext>>(options.XmlRepository);
    }
}
