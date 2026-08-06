using FluentValidation;

namespace Authorization.Application.Accounts.SetAccountActivation;

public sealed class SetAccountActivationCommandValidator : AbstractValidator<SetAccountActivationCommand>
{
    public SetAccountActivationCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEqual(Guid.Empty);
    }
}
