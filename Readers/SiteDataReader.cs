using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.Models;

namespace momentify_backend.Readers;

public interface ISiteDataReader
{
    Task<object?> GetDataAsync(int siteId, TemplateType templateType);
}

public class SiteDataReader : ISiteDataReader
{
    private readonly AppDbContext _context;

    public SiteDataReader(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object?> GetDataAsync(
        int siteId,
        TemplateType templateType)
    {
        return templateType switch
        {
            TemplateType.OurStory =>
                await _context.OurStoryData
                    .FirstOrDefaultAsync(x => x.SiteId == siteId),

            TemplateType.Memories =>
                await _context.MemoriesData
                    .Include(x => x.Memories)
                    .ThenInclude(x => x.Photos)
                    .FirstOrDefaultAsync(x => x.SiteId == siteId),

            _ => throw new InvalidOperationException(
                $"Template data handling is not implemented for {templateType}.")
        };
    }
}

