using Microsoft.AspNetCore.Http;
using momentify_backend.DTOs;

namespace momentify_backend.Services;

public interface IPhotoService
{
    Task<PhotoUploadResponse> UploadAsync(
        int siteId,
        int userId,
        IFormFile image,
        int displayOrder,
        CancellationToken cancellationToken = default);

    Task<PhotoUploadResponse> UpdateAsync(
        int siteId,
        int userId,
        int photoId,
        IFormFile image,
        CancellationToken cancellationToken = default);
}
