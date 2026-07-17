using MediatR;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Commands.CreateReceptionist;

public class CreateReceptionistCommandHandler : IRequestHandler<CreateReceptionistCommand, Guid>
{
    private readonly IAuthorizationServiceClient _authorizationServiceClient;
    private readonly IWorkerRepository _workerRepository;
    private readonly ILogger<CreateReceptionistCommandHandler> _logger;

    public CreateReceptionistCommandHandler(
        IAuthorizationServiceClient authorizationServiceClient,
        IWorkerRepository workerRepository,
        ILogger<CreateReceptionistCommandHandler> logger)
    {
        _authorizationServiceClient = authorizationServiceClient;
        _workerRepository = workerRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateReceptionistCommand request, CancellationToken cancellationToken)
    {
        var accountId = await _authorizationServiceClient.CreateWorkerAccountAsync(
            request.Email, "Receptionist", request.CreatedBy, cancellationToken);

        try
        {
            var receptionist = new Receptionist
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                OfficeId = request.OfficeId,
                PhotoUrl = request.PhotoUrl
            };

            await _workerRepository.AddReceptionistAsync(receptionist, cancellationToken);

            return receptionist.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save receptionist profile after account {AccountId} was created. Compensating.", accountId);

            try
            {
                await _authorizationServiceClient.DeleteWorkerAccountAsync(accountId, CancellationToken.None);
            }
            catch (Exception compensationEx)
            {
                _logger.LogError(compensationEx, "Compensation failed: account {AccountId} may be left orphaned.", accountId);
            }

            throw;
        }
    }
}
