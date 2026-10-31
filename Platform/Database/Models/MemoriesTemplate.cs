using momentify_backend.Platform.Interfaces;

namespace momentify_backend.Platform.Database.Models;

public class MemoriesTemplate : ITemplateDefinition
{
    public TemplateType Type => TemplateType.Memories;

    public string Name => "Memories";

    public Dictionary<string, TemplateInputType> InputRequired => new()
    {
        { "heroTitle", TemplateInputType.Text },
        { "introMessage", TemplateInputType.TextArea },
        { "memories.*.title", TemplateInputType.Text },
        { "memories.*.description", TemplateInputType.TextArea },
        { "memories.*.date", TemplateInputType.Date },
        { "memories.*.displayOrder", TemplateInputType.Number },
        { "memories.*.photos.*.photoId", TemplateInputType.Photo },
        { "finalMessage", TemplateInputType.TextArea }
    };
}