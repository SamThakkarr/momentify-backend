namespace momentify_backend.Platform.Database.Models;

public class MemoryPhoto
{
    public int Id { get; set; }

    public int MemoryId { get; set; }

    public int PhotoId { get; set; }

    public Photo Photo { get; set; } = null!;
}

