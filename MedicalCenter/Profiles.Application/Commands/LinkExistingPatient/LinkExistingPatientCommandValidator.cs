using FluentValidation;
using System;

namespace Profiles.Application.Commands.LinkExistingPatient;

// "Yes, it's me" (US-8, AC-7): привязка существующего профиля к аккаунту
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
