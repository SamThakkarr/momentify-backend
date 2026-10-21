using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.DTOs;
using momentify_backend.Models;
using momentify_backend.Readers;

namespace momentify_backend.Queries.GetUserSites;

public interface IGetUserSitesQueryHandler
{
    Task<List<UserSiteDto>> HandleAsync(int userId);
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

    public async Task<List<UserSiteDto>> HandleAsync(int userId)
    {
        var sites = await _siteReader.GetByUserIdAsync(userId);

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
