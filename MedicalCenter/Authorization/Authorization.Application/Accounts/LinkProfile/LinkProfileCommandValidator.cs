using FluentValidation;

namespace Authorization.Application.Accounts.LinkProfile;

public sealed class LinkProfileCommandValidator : AbstractValidator<LinkProfileCommand>
{
    public LinkProfileCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.ProfileId)
            .NotEqual(Guid.Empty);
    }
}
