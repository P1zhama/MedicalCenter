using Grpc.Core;
using Grpc.Core.Interceptors;
using Serilog.Context;

namespace Authorization.API.Interceptors;

public sealed class CorrelationIdInterceptor : Interceptor
{
    public const string HeaderName = "x-correlation-id";

    private const string LogPropertyName = "CorrelationId";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var correlationId = context.RequestHeaders.GetValue(HeaderName);

        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = Guid.NewGuid().ToString();

        using (LogContext.PushProperty(LogPropertyName, correlationId))
        {
            return await continuation(request, context);
        }
    }
}
