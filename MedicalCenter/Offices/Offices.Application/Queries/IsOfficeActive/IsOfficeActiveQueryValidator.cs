using FluentValidation;

namespace Offices.Application.Queries.IsOfficeActive;

public class IsOfficeActiveQueryValidator : AbstractValidator<IsOfficeActiveQuery>
{
    public IsOfficeActiveQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);
    }
}
