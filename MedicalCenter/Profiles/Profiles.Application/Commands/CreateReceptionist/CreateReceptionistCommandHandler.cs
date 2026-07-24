using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.CreateReceptionist;

public sealed class CreateReceptionistCommandHandler
    : IRequestHandler<CreateReceptionistCommand, ErrorOr<Guid>>
{
    private const string ReceptionistRole = "Receptionist";

    private readonly IAuthorizationServiceClient _authorizationServiceClient;
    private readonly IReceptionistRepository _receptionistRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<CreateReceptionistCommandHandler> _logger;

    public CreateReceptionistCommandHandler(
        IAuthorizationServiceClient authorizationServiceClient,
        IReceptionistRepository receptionistRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<CreateReceptionistCommandHandler> logger)
    {
        _authorizationServiceClient = authorizationServiceClient;
        _receptionistRepository = receptionistRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateReceptionistCommand request, CancellationToken cancellationToken)
    {
        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        Guid.TryParse(request.CreatedBy, out var createdBy);
        var now = _timeProvider.GetUtcNow();

        var accountId = await _authorizationServiceClient.CreateWorkerAccountAsync(
            request.Email, ReceptionistRole, request.CreatedBy, cancellationToken);

        var receptionistResult = Receptionist.Create(
            _guidProvider.NewGuid(),
            accountId,
            nameResult.Value,
            request.OfficeId,
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
