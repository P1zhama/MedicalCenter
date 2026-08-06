using FluentValidation;
using System;

namespace Profiles.Application.Commands.UpdatePatient;

public sealed class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.Id)
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
            .Matches(@"^\+?[0-9]{5,20}$").WithMessage("You've entered an invalid phone number")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Please, select the date");
    }
}
