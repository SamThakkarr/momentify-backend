using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.Platform.Database.Models;

namespace momentify_backend.Platform.Readers;

public interface ITemplateReader
{
    Task<Template?> GetByIdAsync(int templateId);
    Task<List<Template>> GetAllAsync();
}

public class TemplateReader : ITemplateReader
{
    private readonly AppDbContext _context;

    public TemplateReader(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Template?> GetByIdAsync(int templateId)
    {
        return await _context.Templates
            .FirstOrDefaultAsync(x => x.Id == templateId);
    }

    public async Task<List<Template>> GetAllAsync()
    {
        return await _context.Templates
            .OrderBy(x => x.Id)
            .ToListAsync();
    }
}

