using FluentValidation;

namespace Profiles.Application.Commands.CreateMyReceptionistProfile;

public class CreateMyReceptionistProfileCommandValidator : AbstractValidator<CreateMyReceptionistProfileCommand>
{
    public CreateMyReceptionistProfileCommandValidator()
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
