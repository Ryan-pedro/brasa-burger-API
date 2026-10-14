using System.Net;
using System.Net.Http.Headers;
using Brasa.Application.Catalog.Images;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class ImageUploadTests(StoreApiFactory factory)
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];

    [Fact]
    public async Task Imagem_png_e_salva_e_servida_pela_api()
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);

        var image = await (await manager.PostAsync("/api/v1/admin/catalog/images", Upload(Png, "foto.png"))).ReadAsync<ImageResponse>();
        var served = await factory.CreateClient().GetAsync(image.Url);

        Assert.StartsWith("/api/v1/media/", image.Url);
        Assert.Equal(HttpStatusCode.OK, served.StatusCode);
        Assert.Equal(Png, await served.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>", "logo.svg")]
    [InlineData("não sou uma imagem", "disfarce.png")]
    public async Task Arquivo_que_nao_e_imagem_aceita_e_recusado(string content, string fileName)
    {
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);

        var response = await manager.PostAsync("/api/v1/admin/catalog/images", Upload(System.Text.Encoding.UTF8.GetBytes(content), fileName));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task A_cozinha_nao_envia_imagens()
    {
        var kitchen = await factory.SignedInAsync(StoreApiFactory.Kitchen);

        var response = await kitchen.PostAsync("/api/v1/admin/catalog/images", Upload(Png, "foto.png"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static MultipartFormDataContent Upload(byte[] bytes, string fileName)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        return new MultipartFormDataContent { { file, "file", fileName } };
    }
}
