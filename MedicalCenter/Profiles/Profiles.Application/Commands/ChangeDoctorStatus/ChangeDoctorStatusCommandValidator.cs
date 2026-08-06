using FluentValidation;

namespace Profiles.Application.Commands.ChangeDoctorStatus;

public sealed class ChangeDoctorStatusCommandValidator : AbstractValidator<ChangeDoctorStatusCommand>
{
    public ChangeDoctorStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid doctor status");
    }
}
