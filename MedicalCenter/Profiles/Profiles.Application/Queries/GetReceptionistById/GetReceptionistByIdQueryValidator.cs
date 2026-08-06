using FluentValidation;

namespace Profiles.Application.Queries.GetReceptionistById;

public sealed class GetReceptionistByIdQueryValidator : AbstractValidator<GetReceptionistByIdQuery>
{
    public GetReceptionistByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);
    }
}
