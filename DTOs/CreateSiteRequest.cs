using momentify_backend.Models;

namespace momentify_backend.DTOs;

public class CreateSiteRequest
{
    public TemplateType TemplateType { get; set; }

    public object Data { get; set; } = null!;
}
