using FluentValidation;

namespace Services.Application.Queries.IsSpecializationActive;

public class IsSpecializationActiveQueryValidator : AbstractValidator<IsSpecializationActiveQuery>
{
    public IsSpecializationActiveQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);
    }
}
