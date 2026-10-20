using momentify_backend.Data;
using momentify_backend.Interfaces;

namespace momentify_backend.Models;

public class OurStoryData : ITemplate
{
    public int SiteId { get; set; }

    public string HeroSubtitle { get; set; } = null!;
    public string MessageTitle { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string FinalMessage { get; set; } = null!;


    public async Task SaveAsync(AppDbContext context)
    {
        context.OurStoryData.Add(this);
        await context.SaveChangesAsync();
    }
}