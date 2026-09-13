using FluentValidation.TestHelper;
using Offices.Application.Queries.IsOfficeActive;

namespace Offices.UnitTests.Application.Validators;

public sealed class IsOfficeActiveQueryValidatorTests
{
    private readonly IsOfficeActiveQueryValidator _validator = new();

    [Fact]
    public void Validate_NonEmptyId_HasNoErrors()
    {
        var result = _validator.TestValidate(new IsOfficeActiveQuery(new Guid("5f6a7b8c-9d0e-4f1a-8b2c-3d4e5f6a7b13")));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.TestValidate(new IsOfficeActiveQuery(Guid.Empty));

        result.ShouldHaveValidationErrorFor(query => query.Id);
    }
}
