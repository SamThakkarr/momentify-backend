using Google;
using Google.Cloud.Storage.V1;

namespace momentify_backend.Platform.Storage;

public class GcsPhotoStorage : IPhotoStorage
{
    private readonly StorageClient _client;
    private readonly string _bucketName;

    public GcsPhotoStorage(IConfiguration configuration)
    {
        _bucketName = Environment.GetEnvironmentVariable("GCS_BUCKET")
            ?? Environment.GetEnvironmentVariable("PhotoStorage__Bucket")
            ?? configuration["PhotoStorage:Bucket"]
            ?? throw new InvalidOperationException(
                "Set GCS_BUCKET or PhotoStorage:Bucket to the Google Cloud Storage bucket name.");

        _client = StorageClient.Create();
    }

    public async Task<string> SaveAsync(
        int userId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var storageKey = $"uploads/{userId}/{Guid.NewGuid():N}{extension}";

        await _client.UploadObjectAsync(
            _bucketName,
            storageKey,
            contentType,
            content,
            cancellationToken: cancellationToken);

        return storageKey;
    }

    public async Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var destination = new MemoryStream();

        try
        {
            await _client.DownloadObjectAsync(
                _bucketName,
                storageKey,
                destination,
                cancellationToken: cancellationToken);

            destination.Position = 0;
            return destination;
        }
        catch (GoogleApiException exception) when (exception.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await destination.DisposeAsync();
            return null;
        }
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        return _client.DeleteObjectAsync(
            _bucketName,
            storageKey,
            cancellationToken: cancellationToken);
    }
}

