using System.Text.Json;
using momentify_backend.Models;

namespace momentify_backend.Interfaces;

public interface ITemplateValidator
{
    void Validate(
        TemplateType templateType,
        JsonElement data);
}

