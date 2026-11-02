using momentify_backend.Platform.Interfaces;

namespace momentify_backend.Platform.Database.Models;

public class TheBigAskTemplate : ITemplateDefinition
{
    public TemplateType Type => TemplateType.TheBigAsk;

    public string Name => "TheBigAsk";

    public Dictionary<string, TemplateInputType> InputRequired => new()
    {
        { "name", TemplateInputType.Text },
        { "noButtonTexts.*.text", TemplateInputType.Text },
        { "noButtonTexts.*.displayOrder", TemplateInputType.Number },
        { "photoMainPageId", TemplateInputType.Photo },
        { "photoYesPageId", TemplateInputType.Photo }
    };
}
