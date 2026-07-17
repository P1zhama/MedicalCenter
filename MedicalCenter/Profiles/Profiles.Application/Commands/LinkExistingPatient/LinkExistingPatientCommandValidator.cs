using FluentValidation;
using System;

namespace Profiles.Application.Commands.LinkExistingPatient;

public class LinkExistingPatientCommandValidator : AbstractValidator<LinkExistingPatientCommand>
{
    public LinkExistingPatientCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.PatientId)
            .NotEqual(Guid.Empty);
    }
}
