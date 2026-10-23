namespace momentify_backend.Interfaces;

public interface IPhotoStorage
{
    Task<(string StorageKey, string Url)> SaveAsync(
        int siteId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}
