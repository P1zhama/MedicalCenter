using ErrorOr;
using Grpc.Core;
using Offices.Api.ErrorMapping;

namespace Offices.UnitTests.Api;

public sealed class ErrorMappingExtensionsTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCode.InvalidArgument)]
    [InlineData(ErrorType.NotFound, StatusCode.NotFound)]
    [InlineData(ErrorType.Conflict, StatusCode.AlreadyExists)]
    [InlineData(ErrorType.Unauthorized, StatusCode.Unauthenticated)]
    [InlineData(ErrorType.Forbidden, StatusCode.PermissionDenied)]
    [InlineData(ErrorType.Failure, StatusCode.Internal)]
    [InlineData(ErrorType.Unexpected, StatusCode.Internal)]
    public void ToRpcException_ErrorType_MapsToGrpcStatusCode(ErrorType errorType, StatusCode expectedStatusCode)
    {
        List<Error> errors = [Error.Custom((int)errorType, "Office.Test", "Something went wrong.")];

        var exception = errors.ToRpcException();

        exception.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public void ToRpcException_SeveralErrors_TakesStatusCodeFromFirstError()
    {
        List<Error> errors =
        [
            Error.NotFound("Office.NotFound", "Office was not found."),
            Error.Validation("City", "Please, enter the office's city")
        ];

        var exception = errors.ToRpcException();

        exception.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public void ToRpcException_SeveralErrors_JoinsDescriptionsWithSemicolon()
    {
        List<Error> errors =
        [
            Error.Validation("City", "Please, enter the office's city"),
            Error.Validation("Street", "Please, enter the office's street")
        ];

        var exception = errors.ToRpcException();

        exception.Status.Detail.Should().Be("Please, enter the office's city; Please, enter the office's street");
    }
}
