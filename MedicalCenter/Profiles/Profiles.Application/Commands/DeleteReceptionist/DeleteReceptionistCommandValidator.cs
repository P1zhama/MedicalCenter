using FluentValidation;

namespace Profiles.Application.Commands.DeleteReceptionist;

public sealed class DeleteReceptionistCommandValidator : AbstractValidator<DeleteReceptionistCommand>
{
    public DeleteReceptionistCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty);
    }
}
