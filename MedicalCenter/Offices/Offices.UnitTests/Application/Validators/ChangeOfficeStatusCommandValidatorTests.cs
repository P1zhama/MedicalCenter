using FluentValidation.TestHelper;
using Offices.Application.Commands.ChangeOfficeStatus;
using Offices.Domain.Enums;

namespace Offices.UnitTests.Application.Validators;

public sealed class ChangeOfficeStatusCommandValidatorTests
{
    private static readonly Guid OfficeId = new("3d4e5f6a-7b8c-4d9e-8f0a-1b2c3d4e5f12");

    private readonly ChangeOfficeStatusCommandValidator _validator = new();

    [Theory]
    [InlineData(OfficeStatus.Active)]
    [InlineData(OfficeStatus.Inactive)]
    public void Validate_ValidCommand_HasNoErrors(OfficeStatus status)
    {
        var result = _validator.TestValidate(new ChangeOfficeStatusCommand(OfficeId, status));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.TestValidate(new ChangeOfficeStatusCommand(Guid.Empty, OfficeStatus.Active));

        result.ShouldHaveValidationErrorFor(command => command.Id);
    }

    [Theory]
    [InlineData((OfficeStatus)0)]
    [InlineData((OfficeStatus)3)]
    public void Validate_UndefinedStatus_HasInvalidStatusError(OfficeStatus status)
    {
        var result = _validator.TestValidate(new ChangeOfficeStatusCommand(OfficeId, status));

        result.ShouldHaveValidationErrorFor(command => command.Status)
            .WithErrorMessage("Invalid office status");
    }
}
