using FluentValidation;
using System;

namespace Profiles.Application.Commands.ForceCreatePatient;

public class ForceCreatePatientCommandValidator : AbstractValidator<ForceCreatePatientCommand>
{
    public ForceCreatePatientCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Please, enter the first name")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Please, enter the last name")
            .MaximumLength(100);

        RuleFor(x => x.MiddleName)
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Please, enter the phone number")
            .Matches(@"^\+?[0-9]{5,20}$").WithMessage("You've entered an invalid phone number");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth cannot be in the future");
    }
}
