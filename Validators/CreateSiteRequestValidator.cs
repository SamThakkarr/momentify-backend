using System.Text.Json;
using FluentValidation;
using momentify_backend.DTOs;

namespace momentify_backend.Validators;

public class CreateSiteRequestValidator : AbstractValidator<CreateSiteRequest>
{
    public CreateSiteRequestValidator()
    {
        RuleFor(x => x.TemplateType)
            .IsInEnum()
            .WithMessage("Unsupported template type.");

        RuleFor(x => x.Data)
            .Must(x => x.ValueKind == JsonValueKind.Object)
            .WithMessage("Site data must be a JSON object.");
    }
}
