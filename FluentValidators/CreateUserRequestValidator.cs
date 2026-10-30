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

        RuleFor(x => x.PhoneNumber)
            .Must(phone => string.IsNullOrWhiteSpace(phone) ||
                System.Text.RegularExpressions.Regex.IsMatch(
                    phone.Trim(),
                    "^[5-9]\\d{9}$"))
            .WithMessage("Phone number is invalid. It must be 10 digits and start with 5-9.");
    }
}
