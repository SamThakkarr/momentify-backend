using FluentValidation;
using momentify_backend.DTOs;

namespace momentify_backend.FluentValidators;

public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.SecurityKey)
            .NotEmpty();
    }
}

