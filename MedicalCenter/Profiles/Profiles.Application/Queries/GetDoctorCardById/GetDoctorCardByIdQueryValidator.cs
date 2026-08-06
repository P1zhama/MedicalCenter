using FluentValidation;

namespace Profiles.Application.Queries.GetDoctorCardById;

public sealed class GetDoctorCardByIdQueryValidator : AbstractValidator<GetDoctorCardByIdQuery>
{
    public GetDoctorCardByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);
    }
}
