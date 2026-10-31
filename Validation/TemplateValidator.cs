using System.Text.Json;
using momentify_backend.Models;

namespace momentify_backend.Validation;

public static class TemplateValidator
{
    public static void Validate(
        JsonElement data,
        Dictionary<string, TemplateInputType> inputRequired)
    {
        if (data.ValueKind != JsonValueKind.Object)
            throw new ArgumentException(
                "Template data must be a JSON object.");

        foreach (var input in inputRequired)
        {
            if (input.Key.Contains(".*"))
                continue; // Skip nested properties for now
            if (!data.TryGetProperty(
                    input.Key,
                    out var property))
                throw new ArgumentException(
                    $"Required field '{input.Key}' is missing.");

            ValidateInput(
                input.Key,
                property,
                input.Value);
        }
    }

    public static void ValidateInput(
        string propertyName,
        JsonElement property,
        TemplateInputType inputType)
    {
        switch (inputType)
        {
            case TemplateInputType.Text:
            case TemplateInputType.TextArea:
                if (property.ValueKind != JsonValueKind.String)
                    throw new ArgumentException(
                        $"Field '{propertyName}' must be a string.");

                if (string.IsNullOrWhiteSpace(
                        property.GetString()))
                    throw new ArgumentException(
                        $"Field '{propertyName}' cannot be empty.");
                break;

            case TemplateInputType.Date:
                if (property.ValueKind != JsonValueKind.String ||
                    !DateTime.TryParse(
                        property.GetString(),
                        out _))
                    throw new ArgumentException(
                        $"Field '{propertyName}' must be a valid date.");
                break;

            case TemplateInputType.Number:
                if (property.ValueKind != JsonValueKind.Number ||
                    !property.TryGetInt32(out var number) ||
                    number <= 0)
                    throw new ArgumentException(
                        $"Field '{propertyName}' must be a valid positive number.");
                break;

            case TemplateInputType.Photo:
                ValidatePhoto(propertyName, property);
                break;

            case TemplateInputType.PhotoGallery:
                if (property.ValueKind != JsonValueKind.Array)
                    throw new ArgumentException(
                        $"Field '{propertyName}' must be an array.");
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
        if (property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var photoId) ||
            photoId <= 0)
            throw new ArgumentException(
                $"Field '{propertyName}' must contain a valid photo ID.");
    }
}