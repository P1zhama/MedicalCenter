using Common.Abstractions.Tracing;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Serilog.Context;

namespace Common.Api.Interceptors;

public sealed class CorrelationIdInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var correlationId = context.RequestHeaders.GetValue(CorrelationHeaders.CorrelationId);

        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = Guid.NewGuid().ToString();

        using (LogContext.PushProperty(CorrelationHeaders.LogProperty, correlationId))
        {
            return await continuation(request, context);
        }
    }
}
