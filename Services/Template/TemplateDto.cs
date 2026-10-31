using momentify_backend.Platform.Database.Models;

namespace momentify_backend.Services.Template;

public class TemplateDto
{
    public TemplateType Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public string IconId { get; set; } = string.Empty;

    public Dictionary<string, TemplateInputType> Inputs { get; set; } = new();
}

