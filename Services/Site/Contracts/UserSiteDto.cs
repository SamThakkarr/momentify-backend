using momentify_backend.Platform.Database.Models;

namespace momentify_backend.Services.Site.Contracts;

public class UserSiteDto
{
    public int SiteId { get; set; }

    public TemplateType TemplateType { get; set; }

    public string TemplateName { get; set; } = null!;

    public string PublicSlug { get; set; } = null!;

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public object Data { get; set; } = null!;
}