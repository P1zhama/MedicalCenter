using Common.Abstractions.Eventing;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Services.Application.Common.Interfaces;

namespace Services.Application.Commands.ChangeServiceStatus;

public sealed class ChangeServiceStatusCommandHandler : IRequestHandler<ChangeServiceStatusCommand, ErrorOr<Success>>
{
    private readonly IServiceCommandRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public ChangeServiceStatusCommandHandler(
        IServiceCommandRepository serviceRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(ChangeServiceStatusCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (service is null)
            return Error.NotFound("Service.NotFound", "Service was not found.");

        var now = _timeProvider.GetUtcNow();
        var updatedBy = _currentUserProvider.User?.Id ?? Guid.Empty;
        var expectedVersion = service.Version;

        var deactivated = service.ChangeStatus(request.Status, updatedBy, now);
        _serviceRepository.Update(service, expectedVersion);

        if (deactivated)
        {
            await _eventPublisher.PublishAsync(
                new ServiceDeactivatedEvent(service.Id, service.SpecializationId, now.UtcDateTime),
                cancellationToken);
        }

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Service.ConcurrencyConflict", "Service was modified by another operation. Please retry.");

        return Result.Success;
    }
}
