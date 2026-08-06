using FluentValidation;

namespace Services.Application.Queries.GetServiceForAppointment;

public class GetServiceForAppointmentQueryValidator : AbstractValidator<GetServiceForAppointmentQuery>
{
    public GetServiceForAppointmentQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);
    }
}
