using momentify_backend.Readers;
using momentify_backend.Storage;

namespace momentify_backend.Queries.GetPublicPhoto;

public interface IGetPublicPhotoQueryHandler
{
    Task<(Stream Stream, string ContentType)?> HandleAsync(
        int siteId,
        int photoId,
        CancellationToken cancellationToken = default);
}

public class GetPublicPhotoQueryHandler
    : IGetPublicPhotoQueryHandler
{
    private readonly IPhotoReader _photoReader;
    private readonly IPhotoStorage _storage;

    public GetPublicPhotoQueryHandler(
        IPhotoReader photoReader,
        IPhotoStorage storage)
    {
        _photoReader = photoReader;
        _storage = storage;
    }

    public async Task<(Stream Stream, string ContentType)?> HandleAsync(
        int siteId,
        int photoId,
        CancellationToken cancellationToken = default)
    {
        var photo =
            await _photoReader.GetPublishedPhotoAsync(
                photoId,
                siteId,
                cancellationToken);

        if (photo == null)
            return null;

        var stream =
            await _storage.OpenReadAsync(
                photo.StorageKey,
                cancellationToken);

        if (stream == null)
            return null;

        return (
            stream,
            photo.ContentType);
    }
}