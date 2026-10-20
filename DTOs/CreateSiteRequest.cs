using momentify_backend.Models;

namespace momentify_backend.DTOs;

public class CreateSiteRequest
{
    public int UserId { get; set; }

    public TemplateType TemplateType { get; set; }

    public object Data { get; set; } = null!;
}

