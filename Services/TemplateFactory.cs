using System.Text.Json;
using momentify_backend.Interfaces;
using momentify_backend.Models;

namespace momentify_backend.Services;

public class TemplateFactory : ITemplateFactory
{
    public ITemplate Create(
        TemplateType templateType,
        JsonElement data)
    {
        return templateType switch
        {
            TemplateType.OurStory =>
                OurStoryData.Parse(data),

            TemplateType.Memories =>
                MemoriesData.Parse(data),

            _ => throw new InvalidOperationException(
                "Template data handling is not implemented.")
        };
    }
}