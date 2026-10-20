namespace momentify_backend.Models;

public class MemoryPhoto
{
    public int Id { get; set; }

    public int MemoryId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int DisplayOrder { get; set; }
}

