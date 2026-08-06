using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Commands.DeleteReceptionist;

public sealed class DeleteReceptionistCommandHandler : IRequestHandler<DeleteReceptionistCommand, ErrorOr<Success>>
{
    private readonly IReceptionistCommandRepository _receptionistRepository;
    private readonly IAuthorizationServiceClient _authorizationServiceClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReceptionistCommandHandler> _logger;

    public DeleteReceptionistCommandHandler(
        IReceptionistCommandRepository receptionistCommandRepository,
        IAuthorizationServiceClient authorizationServiceClient,
        IUnitOfWork unitOfWork,
        ILogger<DeleteReceptionistCommandHandler> logger)
    {
        _receptionistRepository = receptionistCommandRepository;
        _authorizationServiceClient = authorizationServiceClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> Handle(
        DeleteReceptionistCommand request,
        CancellationToken cancellationToken)
    {
        var receptionist = await _receptionistRepository.GetByIdAsync(request.Id, cancellationToken);
        if (receptionist is null)
            return Error.NotFound("Receptionist.NotFound", "Receptionist was not found.");

        await _authorizationServiceClient.DeleteWorkerAccountAsync(receptionist.AccountId, cancellationToken);

        _receptionistRepository.Remove(receptionist.Id);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Receptionist.ConcurrencyConflict", "Receptionist was modified by another operation. Please retry.");

        _logger.LogInformation(
            "Receptionist {ReceptionistId} deleted along with account {AccountId}",
            receptionist.Id,
            receptionist.AccountId);

        return Result.Success;
    }
}
