namespace momentify_backend.DTOs;

public class PhotoUploadResponse
{
    public int Id { get; set; }

    public string Url { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }
}
