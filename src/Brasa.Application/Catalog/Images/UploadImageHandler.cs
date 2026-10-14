using Brasa.Application.Shared.Results;

namespace Brasa.Application.Catalog.Images;

public sealed record ImageResponse(string Url);

public sealed class UploadImageHandler(IImageStorage storage)
{
    public const int MaxBytes = 2 * 1024 * 1024;

    private static readonly (byte?[] Signature, string Extension)[] Formats =
    [
        ([0xFF, 0xD8, 0xFF], ".jpg"),
        ([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], ".png"),
        ([0x52, 0x49, 0x46, 0x46, null, null, null, null, 0x57, 0x45, 0x42, 0x50], ".webp"),
    ];

    public async Task<Result<ImageResponse>> HandleAsync(Stream content, long length, CancellationToken cancellationToken)
    {
        if (length is <= 0 or > MaxBytes)
        {
            return Invalid("Envie uma imagem de até 2 MB.");
        }

        using var buffer = new MemoryStream((int)length);
        await content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        var extension = DetectExtension(bytes);
        if (extension is null)
        {
            return Invalid("Formato não aceito. Use JPEG, PNG ou WebP.");
        }

        return new ImageResponse(await storage.SaveAsync(bytes, extension, cancellationToken));
    }

    private static string? DetectExtension(byte[] bytes) =>
        Formats
            .Where(format => bytes.Length >= format.Signature.Length
                && format.Signature.Select((expected, index) => expected is null || expected == bytes[index]).All(match => match))
            .Select(format => format.Extension)
            .FirstOrDefault();

    private static Error Invalid(string message)
    {
        var errors = new FieldErrors();
        errors.Add("file", message);
        return errors.ToError();
    }
}
