using momentify_backend.Platform.Database.Models;
using momentify_backend.Platform.Readers;
using momentify_backend.Services.Site.Contracts;

namespace momentify_backend.Platform.Queries.GetSiteBySlug;

public interface IGetSiteBySlugQueryHandler
{
    Task<UserSiteDto?> HandleAsync(string publicSlug);
}

public class GetSiteBySlugQueryHandler : IGetSiteBySlugQueryHandler
{
    private readonly ISiteReader _siteReader;
    private readonly ITemplateReader _templateReader;
    private readonly ISiteDataReader _siteDataReader;

    public GetSiteBySlugQueryHandler(
        ISiteReader siteReader,
        ITemplateReader templateReader,
        ISiteDataReader siteDataReader)
    {
        _siteReader = siteReader;
        _templateReader = templateReader;
        _siteDataReader = siteDataReader;
    }

    public async Task<UserSiteDto?> HandleAsync(string publicSlug)
    {
        var site = await _siteReader.GetBySlugAsync(publicSlug);

        if (site == null || !site.IsPublished)
            return null;

        var template = await _templateReader.GetByIdAsync(site.TemplateId);
        if (template == null)
            return null;

        var templateType = (TemplateType)site.TemplateId;

        var data = await _siteDataReader.GetDataAsync(
            site.Id,
            templateType);

        if (data == null)
            return null;

        return new UserSiteDto
        {
            SiteId = site.Id,
            TemplateType = templateType,
            TemplateName = template.Name,
            PublicSlug = site.PublicSlug,
            IsPublished = site.IsPublished,
            CreatedAt = site.CreatedAt,
            PublishedAt = site.PublishedAt,
            Data = data
        };
    }
}
