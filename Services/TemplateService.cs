using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.DTOs;
using momentify_backend.Interfaces;
using momentify_backend.Models;

namespace momentify_backend.Services;

public class TemplateService : ITemplateService
{
    private readonly AppDbContext _context;

    public TemplateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TemplateDto>> GetAvailableTemplatesAsync()
    {
        var templates = await _context.Templates
            .ToListAsync();

        return templates
            .Where(template => Enum.IsDefined(typeof(TemplateType), template.Id))
            .Select(template =>
            {
                var templateType = (TemplateType)template.Id;
                var templateData = GetTemplateDefinition(templateType);

                return new TemplateDto
                {
                    Type = templateType,
                    Name = template.Name,
                    IconId = template.IconId,
                    Inputs = templateData?.InputRequired ?? new Dictionary<string, TemplateInputType>()
                };
            })
            .ToList();
    }

    private static ITemplateDefinition? GetTemplateDefinition(TemplateType templateType)
    {
        return templateType switch
        {
            TemplateType.OurStory => new OurStoryTemplate(),
            TemplateType.Memories => new MemoriesTemplate(),
            _ => null
        };
    }
}
