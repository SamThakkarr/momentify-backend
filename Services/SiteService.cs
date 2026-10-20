using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.DTOs;
using momentify_backend.Interfaces;
using momentify_backend.Models;

namespace momentify_backend.Services;

public class SiteService : ISiteService
{
    private readonly AppDbContext _context;
    private readonly ITemplateFactory _templateFactory;

    public SiteService(
        AppDbContext context,
        ITemplateFactory templateFactory)
    {
        _context = context;
        _templateFactory = templateFactory;
    }

    public async Task<CreateSiteResponse> CreateSiteAsync(CreateSiteRequest request)
    {
        var userExists = await _context.Users
            .AnyAsync(x => x.Id == request.UserId);

        if (!userExists)
        {
            throw new InvalidOperationException("User does not exist.");
        }

        var template = await _context.Templates
            .FirstOrDefaultAsync(x => x.Id == (int)request.TemplateType);

        if (template == null)
        {
            throw new InvalidOperationException("Template does not exist.");
        }

        ITemplate data = _templateFactory.Create(
            request.TemplateType,
            JsonSerializer.SerializeToElement(request.Data));

        var site = new Site
        {
            UserId = request.UserId,
            TemplateId = (int)request.TemplateType,
            PublicSlug = $"site-{Guid.NewGuid():N}",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Sites.Add(site);
            await _context.SaveChangesAsync();

            data.SiteId = site.Id;
            await data.SaveAsync(_context);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return new CreateSiteResponse
        {
            SiteId = site.Id,
            PublicSlug = site.PublicSlug,
            IsPublished = site.IsPublished
        };
    }

    public async Task<IEnumerable<UserSiteDto>> GetUserSitesAsync(int userId)
    {
        var sites = await _context.Sites
            .Where(site => site.UserId == userId)
            .Join(
                _context.Templates,
                site => site.TemplateId,
                template => template.Id,
                (site, template) => new
                {
                    Site = site,
                    Template = template
                })
            .ToListAsync();

        var result = new List<UserSiteDto>();

        foreach (var item in sites)
        {
            var templateType = (TemplateType)item.Template.Id;

            object? data = templateType switch
            {
                TemplateType.OurStory =>
                    await _context.OurStoryData
                        .FirstOrDefaultAsync(x => x.SiteId == item.Site.Id),

                _ => null
            };

            result.Add(new UserSiteDto
            {
                SiteId = item.Site.Id,
                TemplateType = templateType,
                TemplateName = item.Template.Name,
                PublicSlug = item.Site.PublicSlug,
                IsPublished = item.Site.IsPublished,
                CreatedAt = item.Site.CreatedAt,
                PublishedAt = item.Site.PublishedAt,
                Data = data ?? new { }
            });
        }

        return result;
    }
}
