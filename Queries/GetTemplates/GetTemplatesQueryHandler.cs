using momentify_backend.DTOs;
using momentify_backend.Interfaces;
using momentify_backend.Models;
using momentify_backend.Readers;

namespace momentify_backend.Queries.GetTemplates;

public interface IGetTemplatesQueryHandler
{
    Task<IEnumerable<TemplateDto>> HandleAsync();
}

public class GetTemplatesQueryHandler : IGetTemplatesQueryHandler
{
    private readonly ITemplateReader _templateReader;

    public GetTemplatesQueryHandler(ITemplateReader templateReader)
    {
        _templateReader = templateReader;
    }

    public async Task<IEnumerable<TemplateDto>> HandleAsync()
    {
        var templates = await _templateReader.GetAllAsync();

        return templates
            .Where(template =>
                Enum.IsDefined(typeof(TemplateType), template.Id))
            .Select(template =>
            {
                var templateType = (TemplateType)template.Id;
                var templateDefinition = GetTemplateDefinition(templateType);

                return new TemplateDto
                {
                    Type = templateType,
                    Name = template.Name,
                    IconId = template.IconId,
                    Inputs = templateDefinition?.InputRequired
                        ?? new Dictionary<string, TemplateInputType>()
                };
            })
            .ToList();
    }

    private static ITemplateDefinition? GetTemplateDefinition(
        TemplateType templateType)
    {
        return templateType switch
        {
            TemplateType.OurStory => new OurStoryTemplate(),
            TemplateType.Memories => new MemoriesTemplate(),
            _ => null
        };
    }
}