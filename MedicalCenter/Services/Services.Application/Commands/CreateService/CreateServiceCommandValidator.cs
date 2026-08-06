using FluentValidation;

namespace Services.Application.Commands.CreateService;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please, enter the name")
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("You've entered an invalid price");

        RuleFor(x => x.SpecializationId)
            .NotEqual(Guid.Empty).WithMessage("Please, choose the specialisation");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).WithMessage("Please, choose the service category");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid service status");
    }
}
