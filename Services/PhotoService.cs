using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.DTOs;
using momentify_backend.Interfaces;
using momentify_backend.Models;

namespace momentify_backend.Services;

public class PhotoService : IPhotoService
{
    private const long MaxPhotoSizeBytes = 10 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> AllowedTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
            [".gif"] = "image/gif"
        };

    private readonly AppDbContext _context;
    private readonly IPhotoStorage _storage;

    public PhotoService(
        AppDbContext context,
        IPhotoStorage storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<PhotoUploadResponse> UploadAsync(
        int siteId,
        int userId,
        IFormFile image,
        int displayOrder,
        CancellationToken cancellationToken = default)
    {
        await ValidateSiteOwnershipAsync(
            siteId,
            userId,
            cancellationToken);

        if (displayOrder != -1 && displayOrder < 1)
        {
            throw new ArgumentException(
                "Display order must be -1 for an individual photo or 1 or greater for a gallery photo.");
        }

        var imageInfo = await ValidateImageAsync(
            image,
            cancellationToken);

        string storageKey;
        string url;

        await using (var input = image.OpenReadStream())
        {
            (storageKey, url) = await _storage.SaveAsync(
                siteId,
                imageInfo.FileName,
                imageInfo.ContentType,
                input,
                cancellationToken);
        }

        var photo = new Photo
        {
            StorageKey = storageKey,
            Url = url,
            OriginalFileName = imageInfo.FileName,
            ContentType = imageInfo.ContentType,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Photos.Add(photo);

            await _context.SaveChangesAsync(
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

    public async Task<PhotoUploadResponse> UpdateAsync(
        int siteId,
        int userId,
        int photoId,
        IFormFile image,
        CancellationToken cancellationToken = default)
    {
        await ValidateSiteOwnershipAsync(
            siteId,
            userId,
            cancellationToken);

        var photo = await GetPhotoForSiteAsync(
            siteId,
            photoId,
            cancellationToken);

        if (photo == null)
        {
            throw new KeyNotFoundException(
                "Photo was not found for this site.");
        }

        var imageInfo = await ValidateImageAsync(
            image,
            cancellationToken);

        var oldStorageKey = photo.StorageKey;

        string newStorageKey;
        string newUrl;

        await using (var input = image.OpenReadStream())
        {
            (newStorageKey, newUrl) = await _storage.SaveAsync(
                siteId,
                imageInfo.FileName,
                imageInfo.ContentType,
                input,
                cancellationToken);
        }

        photo.StorageKey = newStorageKey;
        photo.Url = newUrl;
        photo.OriginalFileName = imageInfo.FileName;
        photo.ContentType = imageInfo.ContentType;

        try
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch
        {
            await _storage.DeleteAsync(
                newStorageKey,
                CancellationToken.None);

            throw;
        }

        if (!string.Equals(
                oldStorageKey,
                newStorageKey,
                StringComparison.Ordinal))
        {
            try
            {
                await _storage.DeleteAsync(
                    oldStorageKey,
                    CancellationToken.None);
            }
            catch
            {
                // Leave old physical file if cleanup fails.
            }
        }

        return ToResponse(photo);
    }

    private async Task ValidateSiteOwnershipAsync(
        int siteId,
        int userId,
        CancellationToken cancellationToken)
    {
        var ownsSite = await _context.Sites.AnyAsync(
            site => site.Id == siteId &&
                    site.UserId == userId,
            cancellationToken);

        if (!ownsSite)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to access photos for this site.");
        }
    }

    private async Task<Photo?> GetPhotoForSiteAsync(
        int siteId,
        int photoId,
        CancellationToken cancellationToken)
    {
        var site = await _context.Sites
            .AsNoTracking()
            .Where(x => x.Id == siteId)
            .Select(x => new { x.TemplateId })
            .FirstOrDefaultAsync(cancellationToken);

        if (site == null)
        {
            throw new KeyNotFoundException("Site not found.");
        }

        return site.TemplateId switch
        {
            1 => await GetOurStoryPhotoAsync(
                siteId,
                photoId,
                cancellationToken),

            2 => await GetMemoriesPhotoAsync(
                siteId,
                photoId,
                cancellationToken),

            _ => throw new InvalidOperationException(
                $"Photo handling is not implemented for template {site.TemplateId}.")
        };
    }

    private async Task<Photo?> GetOurStoryPhotoAsync(
        int siteId,
        int photoId,
        CancellationToken cancellationToken)
    {
        var photoIsReferenced = await _context.OurStoryData
            .AnyAsync(
                x => x.SiteId == siteId &&
                     (x.CoverPhotoId == photoId ||
                      x.MemoryPhotoId == photoId),
                cancellationToken);

        if (!photoIsReferenced)
        {
            return null;
        }

        return await _context.Photos
            .FirstOrDefaultAsync(
                x => x.Id == photoId,
                cancellationToken);
    }

    private async Task<Photo?> GetMemoriesPhotoAsync(
        int siteId,
        int photoId,
        CancellationToken cancellationToken)
    {
        var photoIsReferenced =
            await (
                from memoryPhoto in _context.MemoryPhotos
                join memory in _context.Memories
                    on memoryPhoto.MemoryId equals memory.Id
                where memoryPhoto.PhotoId == photoId &&
                      memory.MemoriesDataSiteId == siteId
                select memoryPhoto.Id
            ).AnyAsync(cancellationToken);

        if (!photoIsReferenced)
        {
            return null;
        }

        return await _context.Photos
            .FirstOrDefaultAsync(
                x => x.Id == photoId,
                cancellationToken);
    }

    private static async Task<ImageInfo> ValidateImageAsync(
        IFormFile image,
        CancellationToken cancellationToken)
    {
        if (image == null)
        {
            throw new ArgumentException("An image is required.");
        }

        if (image.Length <= 0 ||
            image.Length > MaxPhotoSizeBytes)
        {
            throw new ArgumentException(
                "The photo must be between 1 byte and 10 MB.");
        }

        var extension = Path.GetExtension(image.FileName);

        if (!AllowedTypes.TryGetValue(
                extension,
                out var expectedContentType) ||
            !string.Equals(
                image.ContentType,
                expectedContentType,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Only JPEG, PNG, WebP, and GIF photos are supported.");
        }

        var safeFileName = Path.GetFileName(image.FileName);

        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException(
                "The uploaded file must have a valid file name.");
        }

        await using var input = image.OpenReadStream();

        if (!await HasValidSignatureAsync(
                input,
                expectedContentType,
                cancellationToken))
        {
            throw new ArgumentException(
                "The uploaded file contents do not match its image type.");
        }

        return new ImageInfo(
            safeFileName,
            expectedContentType);
    }

    private static async Task<bool> HasValidSignatureAsync(
        Stream input,
        string contentType,
        CancellationToken cancellationToken)
    {
        var header = new byte[12];

        var read = await input.ReadAsync(
            header.AsMemory(),
            cancellationToken);

        input.Position = 0;

        return contentType switch
        {
            "image/jpeg" =>
                read >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,

            "image/png" =>
                read >= 8 &&
                header.AsSpan(0, 8).SequenceEqual(
                    new byte[]
                    {
                        0x89, 0x50, 0x4E, 0x47,
                        0x0D, 0x0A, 0x1A, 0x0A
                    }),

            "image/gif" =>
                read >= 6 &&
                System.Text.Encoding.ASCII.GetString(
                    header,
                    0,
                    6) is "GIF87a" or "GIF89a",

            "image/webp" =>
                read >= 12 &&
                System.Text.Encoding.ASCII.GetString(
                    header,
                    0,
                    4) == "RIFF" &&
                System.Text.Encoding.ASCII.GetString(
                    header,
                    8,
                    4) == "WEBP",

            _ => false
        };
    }

    private static PhotoUploadResponse ToResponse(
        Photo photo)
    {
        return new PhotoUploadResponse
        {
            Id = photo.Id,
            Url = photo.Url,
            OriginalFileName = photo.OriginalFileName,
            ContentType = photo.ContentType,
            DisplayOrder = photo.DisplayOrder,
            CreatedAt = photo.CreatedAt
        };
    }

    private sealed record ImageInfo(
        string FileName,
        string ContentType);
}
