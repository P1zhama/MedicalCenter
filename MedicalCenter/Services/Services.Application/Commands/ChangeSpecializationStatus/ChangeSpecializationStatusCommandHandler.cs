using Common.Abstractions.Eventing;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Services.Application.Common.Interfaces;
using Services.Application.Common.Services;

namespace Services.Application.Commands.ChangeSpecializationStatus;

public sealed class ChangeSpecializationStatusCommandHandler
    : IRequestHandler<ChangeSpecializationStatusCommand, ErrorOr<Success>>
{
    private readonly ISpecializationCommandRepository _specializationRepository;
    private readonly SpecializationDeactivation _specializationDeactivation;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public ChangeSpecializationStatusCommandHandler(
        ISpecializationCommandRepository specializationRepository,
        SpecializationDeactivation specializationDeactivation,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _specializationRepository = specializationRepository;
        _specializationDeactivation = specializationDeactivation;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(ChangeSpecializationStatusCommand request, CancellationToken cancellationToken)
    {
        var specialization = await _specializationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (specialization is null)
            return Error.NotFound("Specialization.NotFound", "Specialization was not found.");

        var now = _timeProvider.GetUtcNow();
        var updatedBy = _currentUserProvider.User?.Id ?? Guid.Empty;
        var expectedVersion = specialization.Version;

        var deactivated = specialization.ChangeStatus(request.Status, updatedBy, now);
        _specializationRepository.Update(specialization, expectedVersion);

        var integrationEvents = await _specializationDeactivation.CascadeDeactivationAsync(
            deactivated, specialization, updatedBy, now, cancellationToken);

        foreach (var integrationEvent in integrationEvents)
        {
            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
        }

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Specialization.ConcurrencyConflict", "Specialization was modified by another operation. Please retry.");

        return Result.Success;
    }
}
