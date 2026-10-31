using FluentValidation;
using momentify_backend.Services.User.Contracts;

namespace momentify_backend.Platform.FluentValidators;

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

