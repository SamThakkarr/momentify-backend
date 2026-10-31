using momentify_backend.Platform.Interfaces;

namespace momentify_backend.Platform.Database.Models;

public class OurStoryTemplate : ITemplateDefinition
{
    public TemplateType Type => TemplateType.OurStory;

    public string Name => "Our Story";

    public Dictionary<string, TemplateInputType> InputRequired => new()
    {
        { "heroSubtitle", TemplateInputType.Text },
        { "coverPhotoId", TemplateInputType.Photo },
        { "memoryPhotoId", TemplateInputType.Photo },
        { "messageTitle", TemplateInputType.Text },
        { "message", TemplateInputType.TextArea },
        { "finalMessage", TemplateInputType.TextArea }
    };
}

