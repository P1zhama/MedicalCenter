using Common.Abstractions.Eventing;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Interfaces;
using Profiles.Application.Common.Services;

namespace Profiles.Application.Commands.ChangeDoctorStatus;

public sealed class ChangeDoctorStatusCommandHandler : IRequestHandler<ChangeDoctorStatusCommand, ErrorOr<Success>>
{
    private readonly IDoctorCommandRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public ChangeDoctorStatusCommandHandler(
        IDoctorCommandRepository doctorCommandRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _doctorRepository = doctorCommandRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(ChangeDoctorStatusCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.Id, cancellationToken);
        if (doctor is null)
            return Error.NotFound("Doctor.NotFound", "Doctor was not found.");

        var now = _timeProvider.GetUtcNow();
        var updatedBy = _currentUserProvider.User?.Id ?? Guid.Empty;
        var expectedVersion = doctor.Version;

        var transition = doctor.ChangeStatus(request.Status, updatedBy, now);
        _doctorRepository.Update(doctor, expectedVersion);

        var integrationEvent = WorkerStatusEvents.ForTransition(transition, doctor.AccountId, now);
        if (integrationEvent is not null)
            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Doctor.ConcurrencyConflict", "Doctor was modified by another operation. Please retry.");

        return Result.Success;
    }
}
