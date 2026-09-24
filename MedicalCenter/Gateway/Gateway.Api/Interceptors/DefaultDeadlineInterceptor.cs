using System;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Gateway.Api.Interceptors;

public sealed class DefaultDeadlineInterceptor : Interceptor
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        if (context.Options.Deadline is not null)
            return continuation(request, context);

        var contextWithDeadline = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            context.Options.WithDeadline(DateTime.UtcNow.Add(DefaultTimeout)));

        return continuation(request, contextWithDeadline);
    }
}
