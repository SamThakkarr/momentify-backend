using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.Platform.Database.Models;

namespace momentify_backend.Platform.Readers;

public interface ISiteReader
{
    Task<Site?> GetByIdAsync(int siteId);
    Task<Site?> GetBySlugAsync(string publicSlug);
    Task<List<Site>> GetByUserIdAsync(int userId);
}

public class SiteReader : ISiteReader
{
    private readonly AppDbContext _context;

    public SiteReader(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Site?> GetByIdAsync(int siteId)
    {
        return await _context.Sites
            .FirstOrDefaultAsync(x => x.Id == siteId);
    }

    public async Task<Site?> GetBySlugAsync(string publicSlug)
    {
        return await _context.Sites
            .FirstOrDefaultAsync(x => x.PublicSlug == publicSlug);
    }

    public async Task<List<Site>> GetByUserIdAsync(int userId)
    {
        return await _context.Sites
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}

