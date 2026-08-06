using FluentValidation;
using System;

namespace Profiles.Application.Commands.UpdateReceptionist;

public sealed class UpdateReceptionistCommandValidator : AbstractValidator<UpdateReceptionistCommand>
{
    public UpdateReceptionistCommandValidator()
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

        RuleFor(x => x.OfficeId)
            .NotEqual(Guid.Empty).WithMessage("Please, choose the office");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid receptionist status");
    }
}
