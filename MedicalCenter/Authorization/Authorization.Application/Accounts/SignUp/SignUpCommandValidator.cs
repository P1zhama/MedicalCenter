using FluentValidation;

namespace Authorization.Application.Accounts.SignUp;

public sealed class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    private const int PasswordMinLength = 6;
    private const int PasswordMaxLength = 15;
    private const int EmailMaxLength = 254;

    public SignUpCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Please, enter the email")
            .EmailAddress().WithMessage("You've entered an invalid email")
            .MaximumLength(EmailMaxLength).WithMessage($"Email must be at most {EmailMaxLength} characters");

        RuleFor(command => command.Password)
            .NotEmpty().WithMessage("Please, enter the password")
            .MinimumLength(PasswordMinLength)
                .WithMessage($"Password must be at least {PasswordMinLength} characters")
            .MaximumLength(PasswordMaxLength)
                .WithMessage($"Password must be at most {PasswordMaxLength} characters");
    }
}
