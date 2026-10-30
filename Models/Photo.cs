namespace momentify_backend.Models;

public class Photo
{
    public int Id { get; set; }

    public int UserId { get; set; }


    public string StorageKey { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public int DisplayOrder { get; set; } = -1;

    public DateTime CreatedAt { get; set; }
}