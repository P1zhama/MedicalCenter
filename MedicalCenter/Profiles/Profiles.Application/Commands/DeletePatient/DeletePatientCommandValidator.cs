using FluentValidation;

namespace Profiles.Application.Commands.DeletePatient;

public sealed class DeletePatientCommandValidator : AbstractValidator<DeletePatientCommand>
{
    public DeletePatientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);
    }
}
