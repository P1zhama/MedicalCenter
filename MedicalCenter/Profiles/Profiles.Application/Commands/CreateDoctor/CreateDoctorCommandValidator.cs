using FluentValidation;
using System;

namespace Profiles.Application.Commands.CreateDoctor;

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Please, enter the first name")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Please, enter the last name")
            .MaximumLength(100);

        RuleFor(x => x.MiddleName)
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Please, enter the email")
            .EmailAddress().WithMessage("You've entered an invalid email")
            .MaximumLength(254);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth cannot be in the future");

        RuleFor(x => x.SpecializationId)
            .NotEqual(Guid.Empty).WithMessage("Please, choose the specialization");

        RuleFor(x => x.OfficeId)
            .NotEqual(Guid.Empty).WithMessage("Please, choose the office");

        RuleFor(x => x.CareerStartYear)
            .GreaterThanOrEqualTo(1900)
            .LessThanOrEqualTo(_ => DateTime.UtcNow.Year)
            .WithMessage("Career start year cannot be in the future");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid doctor status");
    }
}
