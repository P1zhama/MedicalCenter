using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Offices.Application.Common.Behaviors;
using Offices.Domain.Constants;
using Offices.UnitTests.Support;

namespace Offices.UnitTests.Application.Behaviors;

public sealed class AuthorizationBehaviorTests
{
    private static readonly Guid HandlerResult = new("5e6f7a8b-9c0d-4e1f-8a2b-3c4d5e6f7a21");

    private readonly ICurrentUserProvider _currentUserProvider = Substitute.For<ICurrentUserProvider>();
    private int _nextCalls;

    private sealed record EditQuery : IRequest<ErrorOr<Guid>>, IAuthorizedRequest
    {
        public string RequiredPermission => Permissions.EditOffice;
    }

    private sealed record EditCommand : IRequest<ErrorOr<Success>>, IAuthorizedRequest
    {
        public string RequiredPermission => Permissions.EditOffice;
    }

    private sealed record OpenQuery : IRequest<ErrorOr<Guid>>;

    private AuthorizationBehavior<TRequest, TResponse> CreateBehavior<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
        where TResponse : IErrorOr
        => new(_currentUserProvider, NullLogger<AuthorizationBehavior<TRequest, TResponse>>.Instance);

    private RequestHandlerDelegate<TResponse> NextReturning<TResponse>(TResponse response)
        => _ =>
        {
            _nextCalls++;
            return Task.FromResult(response);
        };

    private Task<ErrorOr<Guid>> HandleEditQuery(CurrentUser? user)
    {
        _currentUserProvider.User.Returns(user);

        return CreateBehavior<EditQuery, ErrorOr<Guid>>().Handle(
            new EditQuery(),
            NextReturning<ErrorOr<Guid>>(HandlerResult),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_RequestWithoutRequiredPermission_CallsNextEvenForAnonymous()
    {
        _currentUserProvider.User.Returns((CurrentUser?)null);

        var result = await CreateBehavior<OpenQuery, ErrorOr<Guid>>().Handle(
            new OpenQuery(),
            NextReturning<ErrorOr<Guid>>(HandlerResult),
            TestContext.Current.CancellationToken);

        result.Value.Should().Be(HandlerResult);
        _nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NoCurrentUser_ReturnsUnauthorizedWithoutCallingNext()
    {
        var result = await HandleEditQuery(null);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("Auth.Unauthenticated");
        _nextCalls.Should().Be(0);
    }

    [Fact]
    public async Task Handle_UserWithoutId_ReturnsUnauthorizedEvenWithGrantingRole()
    {
        var unauthenticatedReceptionist = new CurrentUser(null, null, [Roles.Receptionist], []);

        var result = await HandleEditQuery(unauthenticatedReceptionist);

        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _nextCalls.Should().Be(0);
    }

    [Fact]
    public async Task Handle_RoleWithoutPermission_ReturnsForbiddenWithoutCallingNext()
    {
        var result = await HandleEditQuery(TestUsers.Doctor);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("Auth.Forbidden");
        _nextCalls.Should().Be(0);
    }

    [Fact]
    public async Task Handle_OtherPermissionGrantedDirectly_ReturnsForbidden()
    {
        var result = await HandleEditQuery(TestUsers.WithPermissions(Permissions.ViewOffices));

        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        _nextCalls.Should().Be(0);
    }

    [Fact]
    public async Task Handle_RoleGrantingPermission_CallsNext()
    {
        var result = await HandleEditQuery(TestUsers.Receptionist);

        result.Value.Should().Be(HandlerResult);
        _nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task Handle_PermissionGrantedDirectly_CallsNext()
    {
        var result = await HandleEditQuery(TestUsers.WithPermissions(Permissions.EditOffice));

        result.Value.Should().Be(HandlerResult);
        _nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task Handle_UnauthenticatedCommandReturningSuccess_ReturnsUnauthorized()
    {
        _currentUserProvider.User.Returns((CurrentUser?)null);

        var result = await CreateBehavior<EditCommand, ErrorOr<Success>>().Handle(
            new EditCommand(),
            NextReturning<ErrorOr<Success>>(Result.Success),
            TestContext.Current.CancellationToken);

        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _nextCalls.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ForbiddenCommandReturningSuccess_ReturnsForbidden()
    {
        _currentUserProvider.User.Returns(TestUsers.Doctor);

        var result = await CreateBehavior<EditCommand, ErrorOr<Success>>().Handle(
            new EditCommand(),
            NextReturning<ErrorOr<Success>>(Result.Success),
            TestContext.Current.CancellationToken);

        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        _nextCalls.Should().Be(0);
    }
}
