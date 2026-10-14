namespace Brasa.Application.Catalog.Images;

public interface IImageStorage
{
    Task<string> SaveAsync(byte[] content, string extension, CancellationToken cancellationToken);
}
