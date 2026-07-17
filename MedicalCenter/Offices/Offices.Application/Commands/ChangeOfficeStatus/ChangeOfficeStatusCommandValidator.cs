using FluentValidation;

namespace Offices.Application.Commands.ChangeOfficeStatus;

public class ChangeOfficeStatusCommandValidator : AbstractValidator<ChangeOfficeStatusCommand>
{
    public ChangeOfficeStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid office status");
    }
}
