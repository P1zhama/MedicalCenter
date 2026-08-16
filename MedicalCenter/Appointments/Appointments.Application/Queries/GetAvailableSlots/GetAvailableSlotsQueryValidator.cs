using FluentValidation;

namespace Appointments.Application.Queries.GetAvailableSlots;

public class GetAvailableSlotsQueryValidator : AbstractValidator<GetAvailableSlotsQuery>
{
    public GetAvailableSlotsQueryValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEqual(Guid.Empty).WithMessage("Please, choose the service");

        RuleFor(x => x.DoctorId)
            .NotEqual(Guid.Empty).When(x => x.DoctorId.HasValue);

        RuleFor(x => x.OfficeId)
            .NotEqual(Guid.Empty).When(x => x.OfficeId.HasValue);
    }
}
