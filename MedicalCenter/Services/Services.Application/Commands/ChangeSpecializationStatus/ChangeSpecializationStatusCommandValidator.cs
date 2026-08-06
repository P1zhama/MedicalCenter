using FluentValidation;

namespace Services.Application.Commands.ChangeSpecializationStatus;

public class ChangeSpecializationStatusCommandValidator : AbstractValidator<ChangeSpecializationStatusCommand>
{
    public ChangeSpecializationStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid specialization status");
    }
}
