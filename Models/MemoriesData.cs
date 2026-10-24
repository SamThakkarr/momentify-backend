using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.Interfaces;
using momentify_backend.Validation;

namespace momentify_backend.Models;

public class MemoriesData : ITemplate
{
    public string HeroTitle { get; set; } = null!;

    public string IntroMessage { get; set; } = null!;

    public string FinalMessage { get; set; } = null!;

    public ICollection<Memory> Memories { get; set; }
        = new List<Memory>();

    public int SiteId { get; set; }

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

    public static MemoriesData Parse(JsonElement data)
    {
        var inputRequired =
            new MemoriesTemplate().InputRequired;

        TemplateValidator.Validate(
            data,
            inputRequired);

        ValidateMemoriesData(data);

        try
        {
            return data.Deserialize<MemoriesData>(
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })
                    ?? throw new InvalidOperationException(
                        "Invalid data provided for the Memories template.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                "Invalid data provided for the Memories template.");
        }
    }

    private static void ValidateMemoriesData(JsonElement data)
    {
        var memories = data.GetProperty("memories");

        var memoryIndex = 0;

        foreach (var memory in memories.EnumerateArray())
        {
            if (memory.ValueKind != JsonValueKind.Object)
                throw new ArgumentException(
                    $"Memory at index {memoryIndex} must be an object.");

            TemplateValidator.ValidateInput(
                $"memories[{memoryIndex}].date",
                memory.GetProperty("date"),
                TemplateInputType.Date);

            TemplateValidator.ValidateInput(
                $"memories[{memoryIndex}].title",
                memory.GetProperty("title"),
                TemplateInputType.Text);

            TemplateValidator.ValidateInput(
                $"memories[{memoryIndex}].description",
                memory.GetProperty("description"),
                TemplateInputType.TextArea);

            TemplateValidator.ValidateInput(
                $"memories[{memoryIndex}].displayOrder",
                memory.GetProperty("displayOrder"),
                TemplateInputType.Number);

            if (!memory.TryGetProperty("photos", out var photos) ||
                photos.ValueKind != JsonValueKind.Array)
                throw new ArgumentException(
                    $"Memory at index {memoryIndex} must contain a photos array.");

            var photoIndex = 0;

            foreach (var photo in photos.EnumerateArray())
            {
                if (photo.ValueKind != JsonValueKind.Object ||
                    !photo.TryGetProperty("photoId", out var photoId))
                    throw new ArgumentException(
                        $"Photo at index {photoIndex} in memory {memoryIndex} must contain a valid photoId.");

                TemplateValidator.ValidateInput(
                    $"memories[{memoryIndex}].photos[{photoIndex}].photoId",
                    photoId,
                    TemplateInputType.Photo);

                photoIndex++;
            }

            memoryIndex++;
        }
    }
}