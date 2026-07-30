using Common.Abstractions.Security;
using Common.Infrastructure.Security;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Common.Api.Interceptors;

public sealed class UserContextInterceptor : Interceptor
{
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

        Guid? userId = Guid.TryParse(headers.GetValue(IdentityHeaders.UserId), out var parsed) ? parsed : null;

        var roles = Split(headers.GetValue(IdentityHeaders.Roles));
        var permissions = Split(headers.GetValue(IdentityHeaders.Permissions));

        _currentUserProvider.Set(new CurrentUser(userId, roles, permissions));

        return continuation(request, context);
    }

    private static IReadOnlyCollection<string> Split(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
