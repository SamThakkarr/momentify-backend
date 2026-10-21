using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.DTOs;
using momentify_backend.Models;
using momentify_backend.Readers;

namespace momentify_backend.Queries.GetSiteBySlug;

public interface IGetSiteBySlugQueryHandler
{
    Task<UserSiteDto?> HandleAsync(string publicSlug);
}

public class GetSiteBySlugQueryHandler : IGetSiteBySlugQueryHandler
{
    private readonly AppDbContext _context;
    private readonly ISiteReader _siteReader;
    private readonly ITemplateReader _templateReader;

    public GetSiteBySlugQueryHandler(
        AppDbContext context,
        ISiteReader siteReader,
        ITemplateReader templateReader)
    {
        _context = context;
        _siteReader = siteReader;
        _templateReader = templateReader;
    }

    public async Task<UserSiteDto?> HandleAsync(string publicSlug)
    {
        var site = await _siteReader.GetBySlugAsync(publicSlug);

        if (site == null || !site.IsPublished)
        {
            return null;
        }

        var template = await _templateReader.GetByIdAsync(site.TemplateId);

        if (template == null)
        {
            return null;
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
                    return null;
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
                    return null;
                }

                data = memoriesData;
                break;
            }

            default:
                throw new InvalidOperationException(
                    $"Template data handling is not implemented for template {site.TemplateId}.");
        }

        return new UserSiteDto
        {
            SiteId = site.Id,
            TemplateType = (TemplateType)site.TemplateId,
            TemplateName = template.Name,
            PublicSlug = site.PublicSlug,
            IsPublished = site.IsPublished,
            CreatedAt = site.CreatedAt,
            PublishedAt = site.PublishedAt,
            Data = data
        };
    }
}
