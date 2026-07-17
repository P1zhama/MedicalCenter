using FluentValidation;

namespace Offices.Application.Commands.CreateOffice;

public class CreateOfficeCommandValidator : AbstractValidator<CreateOfficeCommand>
{
    public CreateOfficeCommandValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Please, enter the office's city")
            .MaximumLength(100);

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Please, enter the office's street")
            .MaximumLength(100);

        RuleFor(x => x.HouseNumber)
            .NotEmpty().WithMessage("Please, enter the office's house number")
            .MaximumLength(20);

        RuleFor(x => x.OfficeNumber)
            .MaximumLength(20);

        RuleFor(x => x.RegistryPhoneNumber)
            .NotEmpty().WithMessage("Please, enter the phone number")
            .Matches(@"^\+?[0-9]{5,20}$").WithMessage("You've entered an invalid phone number");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid office status");
    }
}
