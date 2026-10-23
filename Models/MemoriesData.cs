using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.Interfaces;

namespace momentify_backend.Models;

public class MemoriesData : ITemplate
{
    public int SiteId { get; set; }

    public string HeroTitle { get; set; } = null!;

    public string IntroMessage { get; set; } = null!;

    public string FinalMessage { get; set; } = null!;

    public ICollection<Memory> Memories { get; set; }
        = new List<Memory>();

    public async Task SaveAsync(AppDbContext context)
    {
        var existingData = await context.MemoriesData
            .Include(x => x.Memories)
            .ThenInclude(x => x.Photos)
            .FirstOrDefaultAsync(x => x.SiteId == SiteId);

        if (existingData == null)
        {
            context.MemoriesData.Add(this);

            await context.SaveChangesAsync();
            return;
        }

        existingData.HeroTitle = HeroTitle;
        existingData.IntroMessage = IntroMessage;
        existingData.FinalMessage = FinalMessage;

        context.MemoryPhotos.RemoveRange(
            existingData.Memories.SelectMany(x => x.Photos));

        context.Memories.RemoveRange(
            existingData.Memories);

        existingData.Memories = Memories;

        foreach (var memory in existingData.Memories)
        {
            memory.Id = 0;
            memory.MemoriesDataSiteId = SiteId;

            foreach (var photo in memory.Photos)
            {
                photo.Id = 0;
                photo.MemoryId = 0;
            }
        }

        await context.SaveChangesAsync();
    }
}
