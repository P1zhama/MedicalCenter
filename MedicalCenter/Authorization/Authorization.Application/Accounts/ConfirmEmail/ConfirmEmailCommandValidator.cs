using FluentValidation;

namespace Authorization.Application.Accounts.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(command => command.Token)
            .NotEmpty().WithMessage("Confirmation token is required");
    }
}
