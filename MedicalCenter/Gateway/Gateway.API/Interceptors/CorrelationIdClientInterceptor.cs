using Gateway.Api.Middleware;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;

namespace Gateway.Api.Interceptors;

public sealed class CorrelationIdClientInterceptor : Interceptor
{
    private const string MetadataKey = "x-correlation-id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdClientInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.HeaderName] as string;

        if (string.IsNullOrWhiteSpace(correlationId))
            return continuation(request, context);

        var headers = context.Options.Headers ?? new Metadata();
        headers.Add(MetadataKey, correlationId);

        var options = context.Options.WithHeaders(headers);

        var contextWithCorrelationId = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            options);

        return continuation(request, contextWithCorrelationId);
    }
}
