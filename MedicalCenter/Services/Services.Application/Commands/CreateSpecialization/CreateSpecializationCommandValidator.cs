using Common.Domain;
using FluentValidation;

namespace Services.Application.Commands.CreateSpecialization;

public class CreateSpecializationCommandValidator : AbstractValidator<CreateSpecializationCommand>
{
    public CreateSpecializationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please, enter the name")
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid specialization status");

        RuleFor(x => x.Services)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Please, add at least one service")
            .Must(HaveDistinctNames).WithMessage("Service names must be unique within the specialization");

        RuleForEach(x => x.Services).ChildRules(service =>
        {
            service.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Please, enter the name")
                .MaximumLength(200);

            service.RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("You've entered an invalid price");

            service.RuleFor(x => x.CategoryId)
                .NotEqual(Guid.Empty).WithMessage("Please, choose the service category");

            service.RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid service status");
        });
    }

    private static bool HaveDistinctNames(IReadOnlyList<CreateSpecializationServiceItem> services)
        => services
            .Select(service => TextNormalization.CollapseWhitespace(service.Name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() == services.Count;
}
