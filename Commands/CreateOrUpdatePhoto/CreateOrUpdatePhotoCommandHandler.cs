using momentify_backend.DTOs;
using momentify_backend.Interfaces;
using momentify_backend.Models;
using momentify_backend.Readers;
using momentify_backend.Writers;

namespace momentify_backend.Commands.CreateOrUpdatePhoto;

public interface ICreateOrUpdatePhotoCommandHandler
{
    Task<PhotoUploadResponse> HandleAsync(
        int siteId,
        int userId,
        int? photoId,
        IFormFile image,
        int displayOrder,
        CancellationToken cancellationToken = default);
}

public class CreateOrUpdatePhotoCommandHandler
    : ICreateOrUpdatePhotoCommandHandler
{
    private const long MaxPhotoSizeBytes =
        10 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string>
        AllowedTypes =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg",
                [".png"] = "image/png",
                [".webp"] = "image/webp",
                [".gif"] = "image/gif"
            };

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
        int siteId,
        int userId,
        int? photoId,
        IFormFile image,
        int displayOrder,
        CancellationToken cancellationToken = default)
    {
        var ownsSite =
            await _photoReader.SiteBelongsToUserAsync(
                siteId,
                userId,
                cancellationToken);

        if (!ownsSite)
            throw new KeyNotFoundException(
                "Site not found.");

        if (displayOrder != -1 &&
            displayOrder < 1)
            throw new ArgumentException(
                "Display order must be -1 for an individual photo or 1 or greater for a gallery photo.");

        var imageInfo = await ValidateImageAsync(
            image,
            cancellationToken);

        Photo? existingPhoto = null;

        if (photoId.HasValue)
        {
            existingPhoto =
                await _photoReader.GetByIdForUserAndSiteAsync(
                    photoId.Value,
                    siteId,
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
                    siteId,
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
                SiteId = siteId,
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
        existingPhoto.OriginalFileName =
            imageInfo.FileName;
        existingPhoto.ContentType =
            imageInfo.ContentType;
        existingPhoto.DisplayOrder =
            displayOrder;

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

    private async Task<(string FileName, string ContentType)> ValidateImageAsync(
        IFormFile image,
        CancellationToken cancellationToken)
    {
        if (image == null ||
            image.Length == 0)
            throw new ArgumentException(
                "An image is required.");

        if (image.Length > MaxPhotoSizeBytes)
            throw new ArgumentException(
                "Image size must not exceed 10 MB.");

        var extension =
            Path.GetExtension(image.FileName)
                .ToLowerInvariant();

        if (!AllowedTypes.TryGetValue(
                extension,
                out var expectedContentType))
            throw new ArgumentException(
                "Unsupported image format.");

        if (!string.Equals(
                image.ContentType,
                expectedContentType,
                StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(
                "Invalid image content type.");

        var fileName =
            Path.GetFileName(image.FileName);

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException(
                "Invalid file name.");

        await using var stream =
            image.OpenReadStream();

        if (!await HasValidSignatureAsync(
                stream,
                extension,
                cancellationToken))
            throw new ArgumentException(
                "Invalid image file.");

        return (
            fileName,
            expectedContentType);
    }

    private static async Task<bool> HasValidSignatureAsync(
        Stream stream,
        string extension,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[12];

        var bytesRead = await stream.ReadAsync(
            buffer.AsMemory(0, buffer.Length),
            cancellationToken);

        return extension switch
        {
            ".jpg" or ".jpeg" =>
                bytesRead >= 3 &&
                buffer[0] == 0xFF &&
                buffer[1] == 0xD8 &&
                buffer[2] == 0xFF,

            ".png" =>
                bytesRead >= 8 &&
                buffer[0] == 0x89 &&
                buffer[1] == 0x50 &&
                buffer[2] == 0x4E &&
                buffer[3] == 0x47 &&
                buffer[4] == 0x0D &&
                buffer[5] == 0x0A &&
                buffer[6] == 0x1A &&
                buffer[7] == 0x0A,

            ".gif" =>
                bytesRead >= 6 &&
                buffer[0] == 'G' &&
                buffer[1] == 'I' &&
                buffer[2] == 'F' &&
                buffer[3] == '8' &&
                (buffer[4] == '7' ||
                 buffer[4] == '9') &&
                buffer[5] == 'a',

            ".webp" =>
                bytesRead >= 12 &&
                buffer[0] == 'R' &&
                buffer[1] == 'I' &&
                buffer[2] == 'F' &&
                buffer[3] == 'F' &&
                buffer[8] == 'W' &&
                buffer[9] == 'E' &&
                buffer[10] == 'B' &&
                buffer[11] == 'P',

            _ => false
        };
    }

    private PhotoUploadResponse ToResponse(
        Photo photo)
    {
        return new PhotoUploadResponse
        {
            Id = photo.Id,
            Url = _linkGenerator.GetPathByName(
                "GetPhoto",
                new
                {
                    photo.SiteId,
                    photoId = photo.Id
                }),
            OriginalFileName = photo.OriginalFileName,
            ContentType = photo.ContentType,
            DisplayOrder = photo.DisplayOrder
        };
    }
}