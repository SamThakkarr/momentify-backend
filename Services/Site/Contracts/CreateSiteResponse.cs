namespace momentify_backend.Services.Site.Contracts;

public class CreateSiteResponse
{
    public int SiteId { get; set; }

    public string PublicSlug { get; set; } = null!;

    public bool IsPublished { get; set; }
}

