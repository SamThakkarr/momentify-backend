using momentify_backend.Models;

namespace momentify_backend.Interfaces;

public interface ITemplateDefinition
{
    TemplateType Type { get; }

    string Name { get; }

    Dictionary<string, TemplateInputType> InputRequired { get; }
}

