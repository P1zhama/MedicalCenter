using System.Security.Claims;
using Common.Abstractions.Security;
using Common.Api.Authentication;
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
        var httpContext = context.GetHttpContext();

        if (httpContext.Items.TryGetValue(AuthFailureCodes.HttpContextItem, out var failure) && failure is string code)
            throw new RpcException(new Status(StatusCode.Unauthenticated, code));

        var principal = httpContext.User;

        if (principal.Identity?.IsAuthenticated == true)
            _currentUserProvider.Set(ToCurrentUser(principal));

        return continuation(request, context);
    }

    private static CurrentUser ToCurrentUser(ClaimsPrincipal principal)
    {
        Guid? id = Guid.TryParse(principal.FindFirst(JwtClaimTypes.Subject)?.Value, out var parsed) ? parsed : null;

        var roles = principal.FindAll(JwtClaimTypes.Role).Select(claim => claim.Value).ToArray();
        var permissions = principal.FindAll(JwtClaimTypes.Permission).Select(claim => claim.Value).ToArray();

        return new CurrentUser(id, roles, permissions);
    }
}
