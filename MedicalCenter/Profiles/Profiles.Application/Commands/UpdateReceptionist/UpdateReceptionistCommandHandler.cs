using Common.Abstractions.Eventing;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Interfaces;
using Profiles.Application.Common.Services;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.UpdateReceptionist;

public sealed class UpdateReceptionistCommandHandler
    : IRequestHandler<UpdateReceptionistCommand, ErrorOr<Success>>
{
    private readonly IReceptionistCommandRepository _receptionistRepository;
    private readonly IOfficeServiceClient _officeServiceClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public UpdateReceptionistCommandHandler(
        IReceptionistCommandRepository receptionistCommandRepository,
        IOfficeServiceClient officeServiceClient,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _receptionistRepository = receptionistCommandRepository;
        _officeServiceClient = officeServiceClient;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateReceptionistCommand request,
        CancellationToken cancellationToken)
    {
        var receptionist = await _receptionistRepository.GetByIdAsync(request.Id, cancellationToken);
        if (receptionist is null)
            return Error.NotFound("Receptionist.NotFound", "Receptionist was not found.");

        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        if (receptionist.OfficeId != request.OfficeId
            && !await _officeServiceClient.IsOfficeActiveAsync(request.OfficeId, cancellationToken))
        {
            return Error.Validation("Receptionist.OfficeId", "Please, choose the office");
        }

        var now = _timeProvider.GetUtcNow();
        var updatedBy = _currentUserProvider.User?.Id ?? Guid.Empty;
        var expectedVersion = receptionist.Version;

        var updateResult = receptionist.Update(
            nameResult.Value,
            request.OfficeId,
            request.Status,
            request.PhotoUrl,
            updatedBy,
            now);

        if (updateResult.IsError)
            return updateResult.Errors;

        _receptionistRepository.Update(receptionist, expectedVersion);

        var integrationEvent = WorkerStatusEvents.ForTransition(updateResult.Value, receptionist.AccountId, now);
        if (integrationEvent is not null)
            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Receptionist.ConcurrencyConflict", "Receptionist was modified by another operation. Please retry.");

        return Result.Success;
    }
}
