using Common.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Offices.Api.Interceptors;
using Offices.Api.Protos;
using Offices.UnitTests.Support;

namespace Offices.UnitTests.Api;

public sealed class GrpcExceptionInterceptorTests
{
    private const string MethodName = "/offices.OfficesService/UpdateOffice";

    private readonly FakeLogger<GrpcExceptionInterceptor> _logger = new();
    private readonly GrpcExceptionInterceptor _interceptor;

    public GrpcExceptionInterceptorTests()
    {
        _interceptor = new GrpcExceptionInterceptor(_logger);
    }

    private Task<IsOfficeActiveResponse> Intercept(UnaryServerMethod<IsOfficeActiveRequest, IsOfficeActiveResponse> method)
        => _interceptor.UnaryServerHandler(
            new IsOfficeActiveRequest(),
            TestServerCallContext.Create(MethodName, TestContext.Current.CancellationToken),
            method);

    private Task<IsOfficeActiveResponse> InterceptFailure(Exception exception)
        => Intercept((_, _) => Task.FromException<IsOfficeActiveResponse>(exception));

    [Fact]
    public async Task UnaryServerHandler_MethodSucceeded_ReturnsItsResponse()
    {
        var expected = new IsOfficeActiveResponse { IsActive = true };

        var response = await Intercept((_, _) => Task.FromResult(expected));

        response.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task UnaryServerHandler_RpcException_RethrowsItUnchanged()
    {
        var original = new RpcException(new Status(StatusCode.NotFound, "Office was not found."));

        var act = () => InterceptFailure(original);

        (await act.Should().ThrowAsync<RpcException>()).Which.Should().BeSameAs(original);
    }

    [Fact]
    public async Task UnaryServerHandler_ValidationException_ThrowsInvalidArgumentWithEveryMessage()
    {
        var exception = new ValidationException(
        [
            new ValidationFailure("City", "Please, enter the office's city"),
            new ValidationFailure("Street", "Please, enter the office's street")
        ]);

        var act = () => InterceptFailure(exception);

        var thrown = (await act.Should().ThrowAsync<RpcException>()).Which;
        thrown.StatusCode.Should().Be(StatusCode.InvalidArgument);
        thrown.Status.Detail.Should().Be("Please, enter the office's city; Please, enter the office's street");
    }

    [Fact]
    public async Task UnaryServerHandler_DomainException_ThrowsInvalidArgumentWithItsMessage()
    {
        var act = () => InterceptFailure(new DomainException("city must not be empty."));

        var thrown = (await act.Should().ThrowAsync<RpcException>()).Which;
        thrown.StatusCode.Should().Be(StatusCode.InvalidArgument);
        thrown.Status.Detail.Should().Be("city must not be empty.");
    }

    [Fact]
    public async Task UnaryServerHandler_DomainException_LogsWarning()
    {
        var exception = new DomainException("city must not be empty.");

        var act = () => InterceptFailure(exception);

        await act.Should().ThrowAsync<RpcException>();
        _logger.LatestRecord.Level.Should().Be(LogLevel.Warning);
        _logger.LatestRecord.Exception.Should().BeSameAs(exception);
    }

    [Fact]
    public async Task UnaryServerHandler_KeyNotFoundException_ThrowsNotFound()
    {
        var act = () => InterceptFailure(new KeyNotFoundException("Office was not found."));

        var thrown = (await act.Should().ThrowAsync<RpcException>()).Which;
        thrown.StatusCode.Should().Be(StatusCode.NotFound);
        thrown.Status.Detail.Should().Be("Office was not found.");
    }

    [Theory]
    [InlineData(typeof(FormatException))]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(ArgumentOutOfRangeException))]
    public async Task UnaryServerHandler_MalformedArgument_ThrowsInvalidArgument(Type exceptionType)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Malformed value.")!;

        var act = () => InterceptFailure(exception);

        var thrown = (await act.Should().ThrowAsync<RpcException>()).Which;
        thrown.StatusCode.Should().Be(StatusCode.InvalidArgument);
        thrown.Status.Detail.Should().Be(exception.Message);
    }

    [Fact]
    public async Task UnaryServerHandler_UnexpectedException_ThrowsInternalWithoutItsDetails()
    {
        var exception = new InvalidOperationException("mongodb://offices:secret@mongo:27017 is unreachable");

        var act = () => InterceptFailure(exception);

        var thrown = (await act.Should().ThrowAsync<RpcException>()).Which;
        thrown.StatusCode.Should().Be(StatusCode.Internal);
        thrown.Status.Detail.Should().Be("Internal server error occurred.");
        thrown.Message.Should().NotContain("secret");
    }

    [Fact]
    public async Task UnaryServerHandler_UnexpectedException_LogsErrorWithExceptionAndMethod()
    {
        var exception = new InvalidOperationException("Unexpected failure.");

        var act = () => InterceptFailure(exception);

        await act.Should().ThrowAsync<RpcException>();
        _logger.LatestRecord.Level.Should().Be(LogLevel.Error);
        _logger.LatestRecord.Exception.Should().BeSameAs(exception);
        _logger.LatestRecord.Message.Should().Contain(MethodName);
    }
}
