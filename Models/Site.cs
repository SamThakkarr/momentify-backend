namespace momentify_backend.Models;

public class Site
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TemplateId { get; set; }

    public string PublicSlug { get; set; } = null!;

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }
}
