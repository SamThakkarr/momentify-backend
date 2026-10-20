using System.Text.Json;
using momentify_backend.Models;

namespace momentify_backend.Interfaces;

public interface ITemplateFactory
{
    ITemplate Create(TemplateType templateType, JsonElement data);
}

