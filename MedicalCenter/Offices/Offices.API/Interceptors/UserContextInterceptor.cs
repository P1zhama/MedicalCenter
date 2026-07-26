using Grpc.Core;
using Grpc.Core.Interceptors;
using Offices.Application.Common.Security;
using Offices.Infrastructure.Security;

namespace Offices.API.Interceptors;

public sealed class UserContextInterceptor : Interceptor
{
    public const string UserIdHeader = "x-user-id";

    public const string RolesHeader = "x-user-roles";

    public const string PermissionsHeader = "x-user-permissions";

    private readonly CurrentUserProvider _currentUserProvider;

    public UserContextInterceptor(CurrentUserProvider currentUserProvider)
    {
        _currentUserProvider = currentUserProvider;
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var headers = context.RequestHeaders;

        Guid? userId = Guid.TryParse(headers.GetValue(UserIdHeader), out var parsed) ? parsed : null;

        var roles = Split(headers.GetValue(RolesHeader));
        var permissions = Split(headers.GetValue(PermissionsHeader));

        _currentUserProvider.Set(new CurrentUser(userId, roles, permissions));

        return continuation(request, context);
    }

    private static IReadOnlyCollection<string> Split(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
