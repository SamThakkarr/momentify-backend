using System.Text.Json;
using momentify_backend.Interfaces;
using momentify_backend.Models;

namespace momentify_backend.Services;

public class TemplateFactory : ITemplateFactory
{
    public ITemplate Create(TemplateType templateType, JsonElement data)
    {
        try
        {
            return templateType switch
            {
                TemplateType.OurStory =>
                    data.Deserialize<OurStoryData>(
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })
                    ?? throw new InvalidOperationException(
                        "Invalid data provided for the Our Story template."),

                TemplateType.Memories =>
                    data.Deserialize<MemoriesData>(
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })
                    ?? throw new InvalidOperationException(
                        "Invalid data provided for the Memories template."),

                _ => throw new InvalidOperationException(
                    "Template data handling is not implemented.")
            };
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"Invalid data provided for the {templateType} template.");
        }
    }
}
