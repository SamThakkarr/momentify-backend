using System.Text.Json;
using momentify_backend.Interfaces;
using momentify_backend.Models;

namespace momentify_backend.Services;

public class TemplateValidator : ITemplateValidator
{
    private readonly IEnumerable<ITemplateDefinition> _definitions;

    public TemplateValidator(
        IEnumerable<ITemplateDefinition> definitions)
    {
        _definitions = definitions;
    }

    public void Validate(
        TemplateType templateType,
        JsonElement data)
    {
        var definition = _definitions
            .FirstOrDefault(x => x.Type == templateType);

        if (definition == null)
            throw new InvalidOperationException(
                $"Template definition not found for {templateType}.");

        if (data.ValueKind != JsonValueKind.Object)
            throw new ArgumentException(
                "Template data must be a JSON object.");

        foreach (var input in definition.InputRequired)
        {
            if (!data.TryGetProperty(
                    input.Key,
                    out var property))
            {
                throw new ArgumentException(
                    $"Required field '{input.Key}' is missing.");
            }

            ValidateInput(
                input.Key,
                property,
                input.Value);
        }
    }

    private static void ValidateInput(
        string propertyName,
        JsonElement property,
        TemplateInputType inputType)
    {
        switch (inputType)
        {
            case TemplateInputType.Text:
            case TemplateInputType.TextArea:

                if (property.ValueKind != JsonValueKind.String)
                {
                    throw new ArgumentException(
                        $"Field '{propertyName}' must be a string.");
                }

                if (string.IsNullOrWhiteSpace(
                        property.GetString()))
                {
                    throw new ArgumentException(
                        $"Field '{propertyName}' cannot be empty.");
                }

                break;

            case TemplateInputType.Date:

                if (property.ValueKind != JsonValueKind.String ||
                    !DateTime.TryParse(
                        property.GetString(),
                        out _))
                {
                    throw new ArgumentException(
                        $"Field '{propertyName}' must be a valid date.");
                }

                break;

            case TemplateInputType.Photo:
                ValidatePhoto(propertyName, property);
                break;

            case TemplateInputType.PhotoGallery:

                if (property.ValueKind != JsonValueKind.Array)
                {
                    throw new ArgumentException(
                        $"Field '{propertyName}' must be an array.");
                }

                break;

            case TemplateInputType.Object:

                if (property.ValueKind != JsonValueKind.Object)
                {
                    throw new ArgumentException(
                        $"Field '{propertyName}' must be an object.");
                }

                break;

            case TemplateInputType.ObjectCollection:

                if (property.ValueKind != JsonValueKind.Array)
                {
                    throw new ArgumentException(
                        $"Field '{propertyName}' must be an array.");
                }

                break;

            default:
                throw new ArgumentException(
                    $"Unsupported input type '{inputType}'.");
        }
    }

    private static void ValidatePhoto(
        string propertyName,
        JsonElement property)
    {
        if (property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
        {
            throw new ArgumentException(
                $"Field '{propertyName}' must contain a valid image URL.");
        }
    }
}

