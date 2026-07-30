using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gateway.Api.ErrorHandling;

public sealed class GrpcExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GrpcExceptionHandler> _logger;

    public GrpcExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GrpcExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not RpcException rpcException)
            return false;

        var statusCode = MapStatusCode(rpcException.StatusCode);

        _logger.LogWarning(rpcException, "Downstream gRPC call failed with {GrpcStatus}", rpcException.StatusCode);

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = rpcException,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = ReasonPhrase(statusCode),
                Detail = statusCode >= StatusCodes.Status500InternalServerError
                    ? "Internal server error occurred."
                    : rpcException.Status.Detail
            }
        });
    }

    private static int MapStatusCode(StatusCode grpcStatus) => grpcStatus switch
    {
        StatusCode.InvalidArgument => StatusCodes.Status400BadRequest,
        StatusCode.NotFound => StatusCodes.Status404NotFound,
        StatusCode.AlreadyExists => StatusCodes.Status409Conflict,
        StatusCode.Unauthenticated => StatusCodes.Status401Unauthorized,
        StatusCode.PermissionDenied => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string ReasonPhrase(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "Internal Server Error"
    };
}
