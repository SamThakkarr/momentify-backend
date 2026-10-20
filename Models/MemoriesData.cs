using momentify_backend.Data;
using momentify_backend.Interfaces;

namespace momentify_backend.Models;

public class MemoriesData : ITemplate
{
    public int SiteId { get; set; }

    public string HeroTitle { get; set; } = null!;

    public string IntroMessage { get; set; } = null!;

    public string FinalMessage { get; set; } = null!;

    public ICollection<Memory> Memories { get; set; } = new List<Memory>();

    public async Task SaveAsync(AppDbContext context)
    {
        context.MemoriesData.Add(this);

        await context.SaveChangesAsync();
    }
}

