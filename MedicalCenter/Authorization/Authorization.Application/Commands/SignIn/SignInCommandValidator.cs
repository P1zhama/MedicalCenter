using FluentValidation;

namespace Authorization.Application.Commands.SignIn;

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Please, enter the email")
            .EmailAddress().WithMessage("You've entered an invalid email")
            .MaximumLength(254).WithMessage("Email must be at most 254 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Please, enter the password");

    }
}