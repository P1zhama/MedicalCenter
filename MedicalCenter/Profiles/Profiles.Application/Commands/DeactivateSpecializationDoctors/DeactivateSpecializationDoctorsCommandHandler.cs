using Common.Abstractions.Eventing;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;
using Profiles.Application.Common.Services;

namespace Profiles.Application.Commands.DeactivateSpecializationDoctors;

public sealed class DeactivateSpecializationDoctorsCommandHandler
    : IRequestHandler<DeactivateSpecializationDoctorsCommand, ErrorOr<Success>>
{
    private readonly WorkerDeactivation _workerDeactivation;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DeactivateSpecializationDoctorsCommandHandler> _logger;

    public DeactivateSpecializationDoctorsCommandHandler(
        WorkerDeactivation workerDeactivation,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        TimeProvider timeProvider,
        ILogger<DeactivateSpecializationDoctorsCommandHandler> logger)
    {
        _workerDeactivation = workerDeactivation;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> Handle(
        DeactivateSpecializationDoctorsCommand request,
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();

        var integrationEvents = await _workerDeactivation.CascadeBySpecializationAsync(
            request.SpecializationId, Guid.Empty, now, cancellationToken);

        if (integrationEvents.Count == 0)
            return Result.Success;

        foreach (var integrationEvent in integrationEvents)
        {
            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
        }

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Doctor.ConcurrencyConflict", "Doctors were modified by another operation. Please retry.");

        _logger.LogInformation(
            "Deactivated {Count} doctor(s) of specialization {SpecializationId}",
            integrationEvents.Count,
            request.SpecializationId);

        return Result.Success;
    }
}
