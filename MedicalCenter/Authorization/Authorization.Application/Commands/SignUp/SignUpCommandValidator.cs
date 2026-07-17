using Authorization.Application.Common.Interfaces;
using FluentValidation;


namespace Authorization.Application.Commands.SignUp;

public class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    public SignUpCommandValidator()
    {

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Please, enter the email")
            .EmailAddress().WithMessage("You've entered an invalid email")
            .MaximumLength(254).WithMessage("Email must be at most 254 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Please, enter the password")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(15).WithMessage("Password must be at most 15 characters");
    }
}