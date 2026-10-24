using momentify_backend.Interfaces;

namespace momentify_backend.Services;

public class LocalPhotoStorage : IPhotoStorage
{
    private readonly string _storageRoot;

    public LocalPhotoStorage(IWebHostEnvironment environment)
    {
        _storageRoot = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "uploads");
    }

    public async Task<string> SaveAsync(
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
            true);
        await content.CopyToAsync(output, cancellationToken);

        return storageKey;
    }

    public Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storageKey);

        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            true);

        return Task.FromResult<Stream?>(stream);
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
        var normalizedKey = storageKey.Replace(
            '/',
            Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(
            Path.Combine(_storageRoot, normalizedKey));

        var root = Path.GetFullPath(_storageRoot)
                   + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                root,
                StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Invalid photo storage key.");

        return fullPath;
    }
}