using FluentValidation;
using System;

namespace Profiles.Application.Commands.UpdateMyReceptionistProfile;

public sealed class UpdateMyReceptionistProfileCommandValidator
    : AbstractValidator<UpdateMyReceptionistProfileCommand>
{
    public UpdateMyReceptionistProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Please, enter the first name")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Please, enter the last name")
            .MaximumLength(100);

        RuleFor(x => x.MiddleName)
            .MaximumLength(100);

        RuleFor(x => x.OfficeId)
            .NotEqual(Guid.Empty).WithMessage("Please, choose the office");
    }
}
