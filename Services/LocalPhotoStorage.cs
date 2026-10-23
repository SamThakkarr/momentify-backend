using momentify_backend.Interfaces;

namespace momentify_backend.Services;

public class LocalPhotoStorage : IPhotoStorage
{
    private readonly string _webRootPath;

    public LocalPhotoStorage(IWebHostEnvironment environment)
    {
        _webRootPath = environment.WebRootPath
            ?? Path.Combine(environment.ContentRootPath, "wwwroot");
    }

    public async Task<(string StorageKey, string Url)> SaveAsync(
        int siteId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var storageKey = $"uploads/{siteId}/{Guid.NewGuid():N}{extension}";
        var fullPath = GetFullPath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var output = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true);
        await content.CopyToAsync(output, cancellationToken);

        return (storageKey, $"/{storageKey}");
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storageKey);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private string GetFullPath(string storageKey)
    {
        var normalizedKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_webRootPath, normalizedKey));
        var root = Path.GetFullPath(_webRootPath) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(root, StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid photo storage key.");

        return fullPath;
    }
}
