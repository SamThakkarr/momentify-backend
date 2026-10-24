using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.Interfaces;
using momentify_backend.Services;

namespace momentify_backend.Models;

public class OurStoryData : ITemplate
{
    public int? CoverPhotoId { get; set; }
    public int? MemoryPhotoId { get; set; }
    public Photo? CoverPhoto { get; set; }
    public Photo? MemoryPhoto { get; set; }
    public string HeroSubtitle { get; set; } = null!;
    public string MessageTitle { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string FinalMessage { get; set; } = null!;
    public int SiteId { get; set; }

    public async Task SaveAsync(AppDbContext context)
    {
        var exists = await context.OurStoryData
            .AnyAsync(x => x.SiteId == SiteId);

        if (exists)
            context.OurStoryData.Update(this);
        else
            context.OurStoryData.Add(this);

        await context.SaveChangesAsync();
    }

    public static OurStoryData Parse(JsonElement data)
    {
        var inputRequired =
            new OurStoryTemplate().InputRequired;

        TemplateValidator.Validate(
            data,
            inputRequired);

        try
        {
            return data.Deserialize<OurStoryData>(
                       new JsonSerializerOptions
                       {
                           PropertyNameCaseInsensitive = true
                       })
                   ?? throw new InvalidOperationException(
                       "Invalid data provided for the Our Story template.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                "Invalid data provided for the Our Story template.");
        }
    }
}