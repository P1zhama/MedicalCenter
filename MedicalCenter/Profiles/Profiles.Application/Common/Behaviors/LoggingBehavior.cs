using System.Diagnostics;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Profiles.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            if (response is IErrorOr { IsError: true } failed)
            {
                var errors = failed.Errors ?? [];

                _logger.LogWarning(
                    "{RequestName} failed in {ElapsedMilliseconds} ms with {ErrorCount} error(s): {@Errors}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    errors.Count,
                    errors.Select(error => new { error.Code, error.Type, error.Description }));
            }
            else
            {
                _logger.LogInformation(
                    "{RequestName} completed in {ElapsedMilliseconds} ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            _logger.LogError(
                exception,
                "{RequestName} threw an exception after {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
