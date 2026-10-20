using momentify_backend.Interfaces;

namespace momentify_backend.Models;

public class MemoriesTemplate : ITemplateDefinition
{
    public TemplateType Type => TemplateType.Memories;

    public string Name => "Memories";

    public Dictionary<string, TemplateInputType> InputRequired => new()
    {
        { "heroTitle", TemplateInputType.Text },
        { "introMessage", TemplateInputType.TextArea },
        { "memories", TemplateInputType.ObjectCollection },
        { "finalMessage", TemplateInputType.TextArea }
    };
}

