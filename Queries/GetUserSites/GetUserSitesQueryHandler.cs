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
    private readonly AppDbContext _context;
    private readonly ISiteReader _siteReader;
    private readonly ITemplateReader _templateReader;

    public GetUserSitesQueryHandler(
        AppDbContext context,
        ISiteReader siteReader,
        ITemplateReader templateReader)
    {
        _context = context;
        _siteReader = siteReader;
        _templateReader = templateReader;
    }

    public async Task<List<UserSiteDto>> HandleAsync(int userId)
    {
        var sites = await _siteReader.GetByUserIdAsync(userId);

        var result = new List<UserSiteDto>();

        foreach (var site in sites)
        {
            var template = await _templateReader.GetByIdAsync(site.TemplateId);

            if (template == null)
            {
                continue;
            }

            object data;

            switch ((TemplateType)site.TemplateId)
            {
                case TemplateType.OurStory:
                {
                    var ourStoryData = await _context.OurStoryData
                        .FirstOrDefaultAsync(x => x.SiteId == site.Id);

                    if (ourStoryData == null)
                    {
                        continue;
                    }

                    data = ourStoryData;
                    break;
                }

                case TemplateType.Memories:
                {
                    var memoriesData = await _context.MemoriesData
                        .Include(x => x.Memories)
                            .ThenInclude(x => x.Photos)
                        .FirstOrDefaultAsync(x => x.SiteId == site.Id);

                    if (memoriesData == null)
                    {
                        continue;
                    }

                    data = memoriesData;
                    break;
                }

                default:
                    throw new InvalidOperationException(
                        $"Template data handling is not implemented for template {site.TemplateId}.");
            }

            result.Add(new UserSiteDto
            {
                SiteId = site.Id,
                TemplateType = (TemplateType)site.TemplateId,
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
