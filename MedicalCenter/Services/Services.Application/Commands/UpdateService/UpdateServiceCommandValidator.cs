using FluentValidation;

namespace Services.Application.Commands.UpdateService;

public class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please, enter the name")
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("You've entered an invalid price");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).WithMessage("Please, choose the service category");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid service status");
    }
}
