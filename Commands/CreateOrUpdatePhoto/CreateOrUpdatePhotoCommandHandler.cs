using momentify_backend.DTOs;
using momentify_backend.Models;
using momentify_backend.Readers;
using momentify_backend.Storage;
using momentify_backend.Validation;
using momentify_backend.Writers;

namespace momentify_backend.Commands.CreateOrUpdatePhoto;

public interface ICreateOrUpdatePhotoCommandHandler
{
    Task<PhotoUploadResponse> HandleAsync(
        int userId,
        int? photoId,
        IFormFile image,
        int displayOrder,
        CancellationToken cancellationToken = default);
}

public class CreateOrUpdatePhotoCommandHandler
    : ICreateOrUpdatePhotoCommandHandler
{
    private readonly LinkGenerator _linkGenerator;

    private readonly IPhotoReader _photoReader;
    private readonly IPhotoWriter _photoWriter;
    private readonly IPhotoStorage _storage;

    public CreateOrUpdatePhotoCommandHandler(
        IPhotoReader photoReader,
        IPhotoWriter photoWriter,
        IPhotoStorage storage,
        LinkGenerator linkGenerator)
    {
        _photoReader = photoReader;
        _photoWriter = photoWriter;
        _storage = storage;
        _linkGenerator = linkGenerator;
    }

    public async Task<PhotoUploadResponse> HandleAsync(
        int userId,
        int? photoId,
        IFormFile image,
        int displayOrder,
        CancellationToken cancellationToken = default)
    {
        if (displayOrder != -1 &&
            displayOrder < 1)
            throw new ArgumentException(
                "Display order must be -1 for an individual photo or 1 or greater for a gallery photo.");

        var imageInfo = await PhotoUploadValidator.ValidateAsync(
            image,
            cancellationToken);

        Photo? existingPhoto = null;

        if (photoId is > 0)
        {
            existingPhoto =
                await _photoReader.GetByIdForUserAsync(
                    photoId.Value,
                    userId,
                    cancellationToken);

            if (existingPhoto == null)
                throw new KeyNotFoundException(
                    "Photo not found.");
        }

        string storageKey;

        await using (var input = image.OpenReadStream())
        {
            storageKey =
                await _storage.SaveAsync(
                    userId,
                    imageInfo.FileName,
                    imageInfo.ContentType,
                    input,
                    cancellationToken);
        }

        if (existingPhoto == null)
        {
            var photo = new Photo
            {
                UserId = userId,
                StorageKey = storageKey,
                OriginalFileName = imageInfo.FileName,
                ContentType = imageInfo.ContentType,
                DisplayOrder = displayOrder,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _photoWriter.CreateAsync(
                    photo,
                    cancellationToken);
            }
            catch
            {
                await _storage.DeleteAsync(
                    storageKey,
                    CancellationToken.None);

                throw;
            }

            return ToResponse(photo);
        }

        var oldStorageKey = existingPhoto.StorageKey;

        existingPhoto.StorageKey = storageKey;
        existingPhoto.OriginalFileName = imageInfo.FileName;
        existingPhoto.ContentType = imageInfo.ContentType;
        existingPhoto.DisplayOrder = displayOrder;

        try
        {
            await _photoWriter.SaveAsync(
                cancellationToken);
        }
        catch
        {
            await _storage.DeleteAsync(
                storageKey,
                CancellationToken.None);

            throw;
        }

        try
        {
            await _storage.DeleteAsync(
                oldStorageKey,
                CancellationToken.None);
        }
        catch
        {
            // Database update succeeded.
            // Old file cleanup failure should not
            // make the request fail.
        }

        return ToResponse(existingPhoto);
    }

    private PhotoUploadResponse ToResponse(
        Photo photo)
    {
        var routeUrl = _linkGenerator.GetPathByName(
            "GetPhoto",
            new
            {
                photoId = photo.Id
            });

        return new PhotoUploadResponse
        {
            Id = photo.Id,
            Url = routeUrl ?? string.Empty,
            OriginalFileName = photo.OriginalFileName,
            ContentType = photo.ContentType,
            DisplayOrder = photo.DisplayOrder
        };
    }
}