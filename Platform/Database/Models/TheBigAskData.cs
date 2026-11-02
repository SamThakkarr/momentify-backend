using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.Platform.Interfaces;
using momentify_backend.Platform.Validation;

namespace momentify_backend.Platform.Database.Models;

public class TheBigAskData : ITemplate
{
    public int SiteId { get; set; }
    public string Name { get; set; } = null!;
    public int PhotoMainPageId { get; set; }
    public int PhotoYesPageId { get; set; }
    public ICollection<NoButtonText> NoButtonTexts { get; set; } = new List<NoButtonText>();

    public async Task SaveAsync(AppDbContext context)
    {
        var existing = await context.TheBigAskData
            .Include(x => x.NoButtonTexts)
            .FirstOrDefaultAsync(x => x.SiteId == SiteId);

        if (existing == null)
        {
            context.TheBigAskData.Add(this);
        }
        else
        {
            existing.Name = Name;
            existing.PhotoMainPageId = PhotoMainPageId;
            existing.PhotoYesPageId = PhotoYesPageId;
            context.NoButtonTexts.RemoveRange(existing.NoButtonTexts);
            await context.SaveChangesAsync();
            existing.NoButtonTexts = NoButtonTexts;
        }

        foreach (var text in NoButtonTexts)
            text.SiteId = SiteId;

        await context.SaveChangesAsync();
    }

    public static TheBigAskData Parse(JsonElement data)
    {
        TemplateValidator.Validate(data, new TheBigAskTemplate().InputRequired);

        if (!data.TryGetProperty("noButtonTexts", out var texts) ||
            texts.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Field 'noButtonTexts' must be an array.");

        var index = 0;
        foreach (var item in texts.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object ||
                !item.TryGetProperty("text", out var text) ||
                !item.TryGetProperty("displayOrder", out var displayOrder))
                throw new ArgumentException($"noButtonTexts[{index}] must contain text and displayOrder.");

            TemplateValidator.ValidateInput($"noButtonTexts[{index}].text", text, TemplateInputType.Text);
            TemplateValidator.ValidateInput($"noButtonTexts[{index}].displayOrder", displayOrder, TemplateInputType.Number);
            index++;
        }

        try
        {
            return data.Deserialize<TheBigAskData>(new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Invalid data provided for TheBigAsk.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Invalid data provided for TheBigAsk.");
        }
    }
}
