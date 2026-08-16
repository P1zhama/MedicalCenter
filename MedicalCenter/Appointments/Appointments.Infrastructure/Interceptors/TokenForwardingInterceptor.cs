using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;

namespace Appointments.Infrastructure.Interceptors;

public sealed class TokenForwardingInterceptor : Interceptor
{
    private const string AuthorizationHeader = "authorization";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenForwardingInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(token))
            return continuation(request, context);

        var headers = context.Options.Headers ?? new Metadata();
        headers.Add(AuthorizationHeader, token);

        var forwardedContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            context.Options.WithHeaders(headers));

        return continuation(request, forwardedContext);
    }
}
