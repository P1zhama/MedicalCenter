using Common.Abstractions.Eventing;
using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Domain.Constants;
using Profiles.Domain.Enums;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.CreateMyReceptionistProfile;

public sealed class CreateMyReceptionistProfileCommandHandler
    : IRequestHandler<CreateMyReceptionistProfileCommand, ErrorOr<Guid>>
{
    private readonly IReceptionistQueryRepository _receptionistQueryRepository;
    private readonly IReceptionistCommandRepository _receptionistRepository;
    private readonly IOfficeServiceClient _officeServiceClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<CreateMyReceptionistProfileCommandHandler> _logger;

    public CreateMyReceptionistProfileCommandHandler(
        IReceptionistQueryRepository receptionistQueryRepository,
        IReceptionistCommandRepository receptionistRepository,
        IOfficeServiceClient officeServiceClient,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<CreateMyReceptionistProfileCommandHandler> logger)
    {
        _receptionistQueryRepository = receptionistQueryRepository;
        _receptionistRepository = receptionistRepository;
        _officeServiceClient = officeServiceClient;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(
        CreateMyReceptionistProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.User;

        if (user is null || !user.IsAuthenticated)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        if (!user.Roles.Contains(Roles.Receptionist))
            return Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");

        var accountId = user.Id!.Value;

        var existing = await _receptionistQueryRepository.GetByAccountIdAsync(accountId, cancellationToken);
        if (existing is not null)
            return Error.Conflict("Receptionist.AlreadyExists", "Your profile has already been created.");

        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        if (!await _officeServiceClient.IsOfficeActiveAsync(request.OfficeId, cancellationToken))
            return Error.Validation("Receptionist.OfficeId", "Please, choose the office");

        var now = _timeProvider.GetUtcNow();

        var receptionistResult = Receptionist.Create(
            _guidProvider.NewGuid(),
            accountId,
            nameResult.Value,
            request.OfficeId,
            ReceptionistStatus.Active,
            request.PhotoUrl,
            createdBy: accountId,
            now);

        if (receptionistResult.IsError)
            return receptionistResult.Errors;

        await _receptionistRepository.AddAsync(receptionistResult.Value, cancellationToken);

        await _eventPublisher.PublishAsync(
            new ProfileLinkedToAccountEvent(accountId, receptionistResult.Value.Id, now.UtcDateTime),
            cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Receptionist.ConcurrencyConflict", "Receptionist was modified by another operation. Please retry.");

        _logger.LogInformation(
            "Receptionist {ReceptionistId} created for the signed-in account {AccountId}",
            receptionistResult.Value.Id,
            accountId);

        return receptionistResult.Value.Id;
    }
}
