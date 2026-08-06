using FluentValidation;
using System;

namespace Profiles.Application.Commands.UpdateMyDoctorProfile;

public sealed class UpdateMyDoctorProfileCommandValidator : AbstractValidator<UpdateMyDoctorProfileCommand>
{
    public UpdateMyDoctorProfileCommandValidator()
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
            .WithMessage("Please, select the date");

        RuleFor(x => x.SpecializationId)
            .NotEqual(Guid.Empty).WithMessage("Please, choose the specialisation");

        RuleFor(x => x.OfficeId)
            .NotEqual(Guid.Empty).WithMessage("Please, choose the office");

        RuleFor(x => x.CareerStartYear)
            .GreaterThanOrEqualTo(1900)
            .LessThanOrEqualTo(_ => DateTime.UtcNow.Year)
            .WithMessage("Please, select the year");
    }
}
