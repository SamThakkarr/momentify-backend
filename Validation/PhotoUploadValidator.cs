namespace momentify_backend.Validation;

public static class PhotoUploadValidator
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

    public static async Task<(string FileName, string ContentType)> ValidateAsync(
        IFormFile image,
        CancellationToken cancellationToken = default)
    {
        if (image == null || image.Length == 0)
            throw new ArgumentException("An image is required.");

        if (image.Length > MaxPhotoSizeBytes)
            throw new ArgumentException("Image size must not exceed 10 MB.");

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

        if (!AllowedTypes.TryGetValue(extension, out var expectedContentType))
            throw new ArgumentException("Unsupported image format.");

        if (!string.Equals(
                image.ContentType,
                expectedContentType,
                StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid image content type.");

        var fileName = Path.GetFileName(image.FileName);

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Invalid file name.");

        await using var stream = image.OpenReadStream();

        if (!await HasValidSignatureAsync(stream, extension, cancellationToken))
            throw new ArgumentException("Invalid image file.");

        return (fileName, expectedContentType);
    }

    private static async Task<bool> HasValidSignatureAsync(
        Stream stream,
        string extension,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[12];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

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
                (buffer[4] == '7' || buffer[4] == '9') &&
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
}
