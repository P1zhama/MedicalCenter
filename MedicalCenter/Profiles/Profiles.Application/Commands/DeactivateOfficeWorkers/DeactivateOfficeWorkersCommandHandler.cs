using Common.Abstractions.Eventing;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;
using Profiles.Application.Common.Services;

namespace Profiles.Application.Commands.DeactivateOfficeWorkers;

public sealed class DeactivateOfficeWorkersCommandHandler
    : IRequestHandler<DeactivateOfficeWorkersCommand, ErrorOr<Success>>
{
    private readonly WorkerDeactivation _workerDeactivation;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DeactivateOfficeWorkersCommandHandler> _logger;

    public DeactivateOfficeWorkersCommandHandler(
        WorkerDeactivation workerDeactivation,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        TimeProvider timeProvider,
        ILogger<DeactivateOfficeWorkersCommandHandler> logger)
    {
        _workerDeactivation = workerDeactivation;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> Handle(
        DeactivateOfficeWorkersCommand request,
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();

        var integrationEvents = await _workerDeactivation.CascadeByOfficeAsync(
            request.OfficeId, Guid.Empty, now, cancellationToken);

        if (integrationEvents.Count == 0)
            return Result.Success;

        foreach (var integrationEvent in integrationEvents)
        {
            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
        }

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Worker.ConcurrencyConflict", "Workers were modified by another operation. Please retry.");

        _logger.LogInformation(
            "Deactivated {Count} worker(s) of office {OfficeId}",
            integrationEvents.Count,
            request.OfficeId);

        return Result.Success;
    }
}
