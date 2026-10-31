using System.Text.Json;
using FluentValidation;
using momentify_backend.Services.Site.Contracts;

namespace momentify_backend.Platform.FluentValidators;

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
