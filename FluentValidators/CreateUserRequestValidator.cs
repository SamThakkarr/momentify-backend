using FluentValidation;
using momentify_backend.DTOs;

namespace momentify_backend.FluentValidators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.SecurityKey)
            .NotEmpty()
            .MinimumLength(6);
    }
}

