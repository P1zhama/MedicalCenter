using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.Services;

// Переводит доменные исключения в корректные gRPC-статусы. Без него ошибки валидации
// (FluentValidation) и парсинга (Guid.Parse/DateOnly.ParseExact в ProfilesGrpcService)
// уходили бы клиенту как StatusCode.Unknown (500), а не как понятный InvalidArgument (400)
public class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (ValidationException ex)
        {
            var details = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, details));
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            // Например, битый Guid или дата в неверном формате в запросе
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in gRPC method {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error occurred."));
        }
    }
}
