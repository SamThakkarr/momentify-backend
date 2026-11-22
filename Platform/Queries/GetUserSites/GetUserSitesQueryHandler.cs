using momentify_backend.Platform.Database.Models;
using momentify_backend.Platform.Readers;
using momentify_backend.Services.Site.Contracts;

namespace momentify_backend.Platform.Queries.GetUserSites;

public interface IGetUserSitesQueryHandler
{
    Task<List<UserSiteDto>> HandleAsync(
        int userId,
        int? siteId = null);
}

public class GetUserSitesQueryHandler : IGetUserSitesQueryHandler
{
    private readonly ISiteReader _siteReader;
    private readonly ITemplateReader _templateReader;
    private readonly ISiteDataReader _siteDataReader;

    public GetUserSitesQueryHandler(
        ISiteReader siteReader,
        ITemplateReader templateReader,
        ISiteDataReader siteDataReader)
    {
        _siteReader = siteReader;
        _templateReader = templateReader;
        _siteDataReader = siteDataReader;
    }

    public async Task<List<UserSiteDto>> HandleAsync(
        int userId,
        int? siteId = null)
    {
        List<Site> sites;

        if (siteId.HasValue)
        {
            var site = await _siteReader.GetByIdAsync(siteId.Value);

            if (site == null || site.UserId != userId)
                throw new KeyNotFoundException("Site not found.");

            sites = [site];
        }
        else
        {
            sites = await _siteReader.GetByUserIdAsync(userId);
        }

        var result = new List<UserSiteDto>();

        foreach (var site in sites)
        {
            var template = await _templateReader.GetByIdAsync(site.TemplateId);
            if (template == null)
                continue;

            var templateType = (TemplateType)site.TemplateId;

            var data = await _siteDataReader.GetDataAsync(
                site.Id,
                templateType);

            if (data == null)
                continue;

            result.Add(new UserSiteDto
            {
                SiteId = site.Id,
                TemplateType = templateType,
                TemplateName = template.Name,
                PublicSlug = site.PublicSlug,
                IsPublished = site.IsPublished,
                CreatedAt = site.CreatedAt,
                PublishedAt = site.PublishedAt,
                Data = data
            });
        }

        return result;
    }
}
