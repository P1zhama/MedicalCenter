using FluentValidation;

namespace Services.Application.Commands.UpdateSpecialization;

public class UpdateSpecializationCommandValidator : AbstractValidator<UpdateSpecializationCommand>
{
    public UpdateSpecializationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please, enter the name")
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid specialization status");
    }
}
