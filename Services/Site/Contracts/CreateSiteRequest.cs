using System.Text.Json;
using momentify_backend.Platform.Database.Models;

namespace momentify_backend.Services.Site.Contracts;

public class CreateSiteRequest
{
    public TemplateType TemplateType { get; set; }

    public JsonElement Data { get; set; }
}