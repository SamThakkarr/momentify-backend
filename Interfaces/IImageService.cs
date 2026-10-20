using Microsoft.AspNetCore.Http;

namespace momentify_backend.Interfaces;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile image);

    Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<IFormFile> images);

    Task<bool> DeleteImageAsync(string imageUrl);
}

