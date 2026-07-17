using FluentValidation;
using System;

namespace Profiles.Application.Commands.CreatePatientByReceptionist;

// Правила из US-47 (регистратор заводит пациента офлайн: без email и телефона)
public class CreatePatientByReceptionistCommandValidator : AbstractValidator<CreatePatientByReceptionistCommand>
{
    public CreatePatientByReceptionistCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Please, enter the first name")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Please, enter the last name")
            .MaximumLength(100);

        RuleFor(x => x.MiddleName)
            .MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth cannot be in the future");
    }
}
