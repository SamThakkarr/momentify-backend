using momentify_backend.Platform.Readers;
using momentify_backend.Platform.Storage;

namespace momentify_backend.Platform.Queries.GetPhoto;

public interface IGetPhotoQueryHandler
{
    Task<(Stream Stream, string ContentType)?> HandleAsync(
        int siteId,
        int userId,
        int photoId,
        CancellationToken cancellationToken = default);
}

public class GetPhotoQueryHandler
    : IGetPhotoQueryHandler
{
    private readonly IPhotoReader _photoReader;
    private readonly IPhotoStorage _storage;

    public GetPhotoQueryHandler(
        IPhotoReader photoReader,
        IPhotoStorage storage)
    {
        _photoReader = photoReader;
        _storage = storage;
    }

    public async Task<(Stream Stream, string ContentType)?> HandleAsync(
        int siteId,
        int userId,
        int photoId,
        CancellationToken cancellationToken = default)
    {
        var photo =
            await _photoReader.GetByIdForUserAndSiteAsync(
                photoId,
                siteId,
                userId,
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