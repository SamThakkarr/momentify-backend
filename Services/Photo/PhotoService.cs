using momentify_backend.Commands.CreateOrUpdatePhoto;
using momentify_backend.Platform.Queries.GetPhoto;
using momentify_backend.Platform.Queries.GetPublicPhoto;
using momentify_backend.Services.Photo.Contracts;

namespace momentify_backend.Services.Photo;

public interface IPhotoService
{
    Task<PhotoUploadResponse> UploadAsync(
        int userId,
        int? photoId,
        IFormFile image,
        int displayOrder,
        CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType)?> GetAsync(
        int siteId,
        int userId,
        int photoId,
        CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType)?> PublicGetAsync(
        int siteId,
        int photoId,
        CancellationToken cancellationToken = default);
}

public class PhotoService : IPhotoService
{
    private readonly ICreateOrUpdatePhotoCommandHandler _createOrUpdatePhotoHandler;
    private readonly IGetPhotoQueryHandler _getPhotoHandler;
    private readonly IGetPublicPhotoQueryHandler _getPublicPhotoHandler;

    public PhotoService(
        ICreateOrUpdatePhotoCommandHandler createOrUpdatePhotoHandler,
        IGetPhotoQueryHandler getPhotoHandler,
        IGetPublicPhotoQueryHandler getPublicPhotoHandler)
    {
        _createOrUpdatePhotoHandler = createOrUpdatePhotoHandler;
        _getPhotoHandler = getPhotoHandler;
        _getPublicPhotoHandler = getPublicPhotoHandler;
    }

    public async Task<PhotoUploadResponse> UploadAsync(
        int userId,
        int? photoId,
        IFormFile image,
        int displayOrder,
        CancellationToken cancellationToken = default)
    {
        return await _createOrUpdatePhotoHandler.HandleAsync(
            userId,
            photoId,
            image,
            displayOrder,
            cancellationToken);
    }

    public async Task<(Stream Stream, string ContentType)?> GetAsync(
        int siteId,
        int userId,
        int photoId,
        CancellationToken cancellationToken = default)
    {
        return await _getPhotoHandler.HandleAsync(
            siteId,
            userId,
            photoId,
            cancellationToken);
    }

    public async Task<(Stream Stream, string ContentType)?> PublicGetAsync(
        int siteId,
        int photoId,
        CancellationToken cancellationToken = default)
    {
        return await _getPublicPhotoHandler.HandleAsync(
            siteId,
            photoId,
            cancellationToken);
    }
}