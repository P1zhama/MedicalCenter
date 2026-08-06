using Common.Abstractions.Eventing;
using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Domain.Enums;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.CreateReceptionist;

public sealed class CreateReceptionistCommandHandler
    : IRequestHandler<CreateReceptionistCommand, ErrorOr<Guid>>
{
    private const string ReceptionistRole = "Receptionist";

    private readonly IAuthorizationServiceClient _authorizationServiceClient;
    private readonly IOfficeServiceClient _officeServiceClient;
    private readonly IReceptionistCommandRepository _receptionistRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<CreateReceptionistCommandHandler> _logger;

    public CreateReceptionistCommandHandler(
        IAuthorizationServiceClient authorizationServiceClient,
        IOfficeServiceClient officeServiceClient,
        IReceptionistCommandRepository receptionistCommandRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<CreateReceptionistCommandHandler> logger)
    {
        _authorizationServiceClient = authorizationServiceClient;
        _officeServiceClient = officeServiceClient;
        _receptionistRepository = receptionistCommandRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateReceptionistCommand request, CancellationToken cancellationToken)
    {
        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        if (!await _officeServiceClient.IsOfficeActiveAsync(request.OfficeId, cancellationToken))
            return Error.Validation("Receptionist.OfficeId", "Please, choose the office");

        var now = _timeProvider.GetUtcNow();
        var createdBy = _currentUserProvider.User?.Id ?? Guid.Empty;

        var accountId = await _authorizationServiceClient.CreateWorkerAccountAsync(
            request.Email, ReceptionistRole, createdBy, cancellationToken);

        var receptionistResult = Receptionist.Create(
            _guidProvider.NewGuid(),
            accountId,
            nameResult.Value,
            request.OfficeId,
            ReceptionistStatus.Active,
            request.PhotoUrl,
            createdBy,
            now);

        if (receptionistResult.IsError)
        {
            await CompensateAsync(accountId);
            return receptionistResult.Errors;
        }

        try
        {
            await _receptionistRepository.AddAsync(receptionistResult.Value, cancellationToken);

            await _eventPublisher.PublishAsync(
                new ProfileLinkedToAccountEvent(accountId, receptionistResult.Value.Id, now.UtcDateTime),
                cancellationToken);

            if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            {
                await CompensateAsync(accountId);

                return Error.Conflict("Receptionist.ConcurrencyConflict", "Receptionist was modified by another operation. Please retry.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to persist receptionist profile after account {AccountId} was created. Compensating.",
                accountId);

            await CompensateAsync(accountId);

            throw;
        }

        _logger.LogInformation(
            "Receptionist {ReceptionistId} created for account {AccountId}",
            receptionistResult.Value.Id,
            accountId);

        return receptionistResult.Value.Id;
    }

    private async Task CompensateAsync(Guid accountId)
    {
        try
        {
            await _authorizationServiceClient.DeleteWorkerAccountAsync(accountId, CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Compensation failed: account {AccountId} may be left orphaned.", accountId);
        }
    }
}
