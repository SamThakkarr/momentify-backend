namespace momentify_backend.Models;

public class Site
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid TemplateId { get; set; }

    public string PublicSlug { get; set; } = null!;

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public User User { get; set; } = null!;

    public Template Template { get; set; } = null!;
}

