using System.Text.Json;
using momentify_backend.Platform.Database.Models;

namespace momentify_backend.Platform.Interfaces;

public interface ITemplateFactory
{
    ITemplate Create(TemplateType templateType, JsonElement data);
}

