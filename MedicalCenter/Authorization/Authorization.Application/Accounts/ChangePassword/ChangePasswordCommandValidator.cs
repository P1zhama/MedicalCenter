using FluentValidation;

namespace Authorization.Application.Accounts.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    private const int PasswordMinLength = 6;
    private const int PasswordMaxLength = 15;

    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.CurrentPassword)
            .NotEmpty().WithMessage("Please, enter the password");

        RuleFor(command => command.NewPassword)
            .NotEmpty().WithMessage("Please, enter the password")
            .MinimumLength(PasswordMinLength)
                .WithMessage($"Password must be at least {PasswordMinLength} characters")
            .MaximumLength(PasswordMaxLength)
                .WithMessage($"Password must be at most {PasswordMaxLength} characters");

        RuleFor(command => command.ConfirmNewPassword)
            .NotEmpty().WithMessage("Please, repeat the password")
            .Equal(command => command.NewPassword).WithMessage("Passwords must match");
    }
}
