namespace momentify_backend.Platform.Database.Models;

public class Memory
{
    public int Id { get; set; }

    public int MemoriesDataSiteId { get; set; }

    public DateTime Date { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public ICollection<MemoryPhoto> Photos { get; set; } = new List<MemoryPhoto>();
}