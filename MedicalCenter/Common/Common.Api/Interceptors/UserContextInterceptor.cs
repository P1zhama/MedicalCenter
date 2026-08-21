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

        if (principal.IsAuthenticated())
            _currentUserProvider.Set(principal.ToCurrentUser());

        return continuation(request, context);
    }
}
