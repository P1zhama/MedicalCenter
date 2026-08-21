using FluentValidation;

namespace Documents.Application.Commands.UploadDocument;

public sealed class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public const long MaxSizeBytes = 5L * 1024 * 1024;

    public UploadDocumentCommandValidator()
    {
        RuleFor(command => command.Kind)
            .IsInEnum()
            .WithMessage("Document kind is unknown.");

        RuleFor(command => command.FileName)
            .NotEmpty()
            .WithMessage("File name is required.")
            .MaximumLength(255)
            .WithMessage("File name must not exceed 255 characters.");

        RuleFor(command => command.Size)
            .GreaterThan(0)
            .WithMessage("File must not be empty.")
            .LessThanOrEqualTo(MaxSizeBytes)
            .WithMessage("File must not exceed 5 MB.");

        RuleFor(command => command.Content)
            .NotNull()
            .WithMessage("File content is required.");
    }
}
