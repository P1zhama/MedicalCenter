using Common.Api.Authentication;
using Common.Infrastructure.Security;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Common.Api.Interceptors;

public sealed class OptionalUserContextInterceptor : Interceptor
{
    private readonly CurrentUserProvider _currentUserProvider;

    public OptionalUserContextInterceptor(CurrentUserProvider currentUserProvider)
    {
        _currentUserProvider = currentUserProvider;
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var principal = context.GetHttpContext().User;

        if (principal.IsAuthenticated())
            _currentUserProvider.Set(principal.ToCurrentUser());

        return continuation(request, context);
    }
}
