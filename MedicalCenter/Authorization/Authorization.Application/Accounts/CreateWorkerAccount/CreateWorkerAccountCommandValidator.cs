using FluentValidation;

namespace Authorization.Application.Accounts.CreateWorkerAccount;

public sealed class CreateWorkerAccountCommandValidator : AbstractValidator<CreateWorkerAccountCommand>
{
    private const int EmailMaxLength = 254;

    public CreateWorkerAccountCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Please, enter the email")
            .EmailAddress().WithMessage("You've entered an invalid email")
            .MaximumLength(EmailMaxLength).WithMessage($"Email must be at most {EmailMaxLength} characters");

        RuleFor(command => command.RoleName)
            .NotEmpty().WithMessage("Role is required");
    }
}
