using momentify_backend.Interfaces;

namespace momentify_backend.Models;

public class OurStoryData : ITemplate
{
    public int SiteId { get; set; }

    public string HeroSubtitle { get; set; } = null!;

    public string MessageTitle { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string FinalMessage { get; set; } = null!;

    public TemplateType Type => TemplateType.OurStory;

    public string Name => "Our Story";

    public Dictionary<string, TemplateInputType> InputRequired => new()
    {
        { nameof(HeroSubtitle), TemplateInputType.Text },
        { nameof(MessageTitle), TemplateInputType.Text },
        { nameof(Message), TemplateInputType.TextArea },
        { nameof(FinalMessage), TemplateInputType.TextArea }
    };
}