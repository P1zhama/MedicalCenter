using FluentValidation.TestHelper;
using Offices.Application.Commands.CreateOffice;
using Offices.Domain.Enums;

namespace Offices.UnitTests.Application.Validators;

public sealed class CreateOfficeCommandValidatorTests
{
    private static readonly CreateOfficeCommand ValidCommand = new(
        City: "Москва",
        Street: "Тверская",
        HouseNumber: "12",
        OfficeNumber: "101",
        RegistryPhoneNumber: "+74951234567",
        PhotoUrl: "/api/documents/7c1d2e3f-4a5b-4c6d-8e9f-0a1b2c3d4e10",
        Status: OfficeStatus.Active);

    private readonly CreateOfficeCommandValidator _validator = new();

    [Theory]
    [InlineData(OfficeStatus.Active)]
    [InlineData(OfficeStatus.Inactive)]
    public void Validate_ValidCommand_HasNoErrors(OfficeStatus status)
    {
        var result = _validator.TestValidate(ValidCommand with { Status = status });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_CommandWithoutOptionalFields_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand with { OfficeNumber = null, PhotoUrl = null });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankCity_HasRequiredError(string city)
    {
        var result = _validator.TestValidate(ValidCommand with { City = city });

        result.ShouldHaveValidationErrorFor(command => command.City)
            .WithErrorMessage("Please, enter the office's city");
    }

    [Fact]
    public void Validate_CityOf101Characters_HasError()
    {
        var result = _validator.TestValidate(ValidCommand with { City = new string('г', 101) });

        result.ShouldHaveValidationErrorFor(command => command.City);
    }

    [Fact]
    public void Validate_CityOf100Characters_HasNoError()
    {
        var result = _validator.TestValidate(ValidCommand with { City = new string('г', 100) });

        result.ShouldNotHaveValidationErrorFor(command => command.City);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankStreet_HasRequiredError(string street)
    {
        var result = _validator.TestValidate(ValidCommand with { Street = street });

        result.ShouldHaveValidationErrorFor(command => command.Street)
            .WithErrorMessage("Please, enter the office's street");
    }

    [Fact]
    public void Validate_StreetOf101Characters_HasError()
    {
        var result = _validator.TestValidate(ValidCommand with { Street = new string('у', 101) });

        result.ShouldHaveValidationErrorFor(command => command.Street);
    }

    [Fact]
    public void Validate_StreetOf100Characters_HasNoError()
    {
        var result = _validator.TestValidate(ValidCommand with { Street = new string('у', 100) });

        result.ShouldNotHaveValidationErrorFor(command => command.Street);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankHouseNumber_HasRequiredError(string houseNumber)
    {
        var result = _validator.TestValidate(ValidCommand with { HouseNumber = houseNumber });

        result.ShouldHaveValidationErrorFor(command => command.HouseNumber)
            .WithErrorMessage("Please, enter the office's house number");
    }

    [Fact]
    public void Validate_HouseNumberOf21Characters_HasError()
    {
        var result = _validator.TestValidate(ValidCommand with { HouseNumber = new string('1', 21) });

        result.ShouldHaveValidationErrorFor(command => command.HouseNumber);
    }

    [Fact]
    public void Validate_HouseNumberOf20Characters_HasNoError()
    {
        var result = _validator.TestValidate(ValidCommand with { HouseNumber = new string('1', 20) });

        result.ShouldNotHaveValidationErrorFor(command => command.HouseNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_MissingOfficeNumber_HasNoError(string? officeNumber)
    {
        var result = _validator.TestValidate(ValidCommand with { OfficeNumber = officeNumber });

        result.ShouldNotHaveValidationErrorFor(command => command.OfficeNumber);
    }

    [Fact]
    public void Validate_OfficeNumberOf21Characters_HasError()
    {
        var result = _validator.TestValidate(ValidCommand with { OfficeNumber = new string('1', 21) });

        result.ShouldHaveValidationErrorFor(command => command.OfficeNumber);
    }

    [Fact]
    public void Validate_OfficeNumberOf20Characters_HasNoError()
    {
        var result = _validator.TestValidate(ValidCommand with { OfficeNumber = new string('1', 20) });

        result.ShouldNotHaveValidationErrorFor(command => command.OfficeNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankPhone_HasRequiredError(string phoneNumber)
    {
        var result = _validator.TestValidate(ValidCommand with { RegistryPhoneNumber = phoneNumber });

        result.ShouldHaveValidationErrorFor(command => command.RegistryPhoneNumber)
            .WithErrorMessage("Please, enter the phone number");
    }

    [Theory]
    [MemberData(nameof(PhoneNumberCases.Malformed), MemberType = typeof(PhoneNumberCases))]
    public void Validate_MalformedPhone_HasInvalidPhoneError(string phoneNumber)
    {
        var result = _validator.TestValidate(ValidCommand with { RegistryPhoneNumber = phoneNumber });

        result.ShouldHaveValidationErrorFor(command => command.RegistryPhoneNumber)
            .WithErrorMessage("You've entered an invalid phone number");
    }

    [Theory]
    [MemberData(nameof(PhoneNumberCases.WellFormed), MemberType = typeof(PhoneNumberCases))]
    public void Validate_WellFormedPhone_HasNoError(string phoneNumber)
    {
        var result = _validator.TestValidate(ValidCommand with { RegistryPhoneNumber = phoneNumber });

        result.ShouldNotHaveValidationErrorFor(command => command.RegistryPhoneNumber);
    }

    [Theory]
    [InlineData((OfficeStatus)0)]
    [InlineData((OfficeStatus)3)]
    public void Validate_UndefinedStatus_HasInvalidStatusError(OfficeStatus status)
    {
        var result = _validator.TestValidate(ValidCommand with { Status = status });

        result.ShouldHaveValidationErrorFor(command => command.Status)
            .WithErrorMessage("Invalid office status");
    }
}
