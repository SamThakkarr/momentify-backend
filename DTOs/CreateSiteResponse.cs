namespace momentify_backend.DTOs;

public class CreateSiteResponse
{
    public int SiteId { get; set; }

    public string PublicSlug { get; set; } = null!;

    public bool IsPublished { get; set; }
}

