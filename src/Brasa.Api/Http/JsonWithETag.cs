using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Brasa.Api.Http;

internal static class JsonWithETag
{
    public static IResult Create<T>(HttpContext http, T value)
    {
        var options = http.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
        var body = JsonSerializer.SerializeToUtf8Bytes(value, options);
        var etag = $"\"{Convert.ToHexStringLower(SHA256.HashData(body))[..32]}\"";

        http.Response.Headers.ETag = etag;
        http.Response.Headers.CacheControl = "no-cache";

        return http.Request.Headers.IfNoneMatch.Contains(etag)
            ? Results.StatusCode(StatusCodes.Status304NotModified)
            : Results.Bytes(body, "application/json");
    }
}
