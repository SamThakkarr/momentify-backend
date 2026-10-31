using momentify_backend.Platform.Database.Models;

namespace momentify_backend.Platform.Interfaces;

public interface ITemplateDefinition
{
    TemplateType Type { get; }

    string Name { get; }

    Dictionary<string, TemplateInputType> InputRequired { get; }
}

