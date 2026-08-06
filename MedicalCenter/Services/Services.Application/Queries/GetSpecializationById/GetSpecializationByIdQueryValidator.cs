using FluentValidation;

namespace Services.Application.Queries.GetSpecializationById;

public class GetSpecializationByIdQueryValidator : AbstractValidator<GetSpecializationByIdQuery>
{
    public GetSpecializationByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);
    }
}
