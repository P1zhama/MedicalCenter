using FluentValidation;

namespace Services.Application.Commands.ChangeServiceStatus;

public class ChangeServiceStatusCommandValidator : AbstractValidator<ChangeServiceStatusCommand>
{
    public ChangeServiceStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid service status");
    }
}
