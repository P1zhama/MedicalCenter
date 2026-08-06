using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Commands.DeletePatient;

public sealed class DeletePatientCommandHandler : IRequestHandler<DeletePatientCommand, ErrorOr<Success>>
{
    private readonly IPatientCommandRepository _patientRepository;
    private readonly IAuthorizationServiceClient _authorizationServiceClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePatientCommandHandler> _logger;

    public DeletePatientCommandHandler(
        IPatientCommandRepository patientCommandRepository,
        IAuthorizationServiceClient authorizationServiceClient,
        IUnitOfWork unitOfWork,
        ILogger<DeletePatientCommandHandler> logger)
    {
        _patientRepository = patientCommandRepository;
        _authorizationServiceClient = authorizationServiceClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.Id, cancellationToken);
        if (patient is null)
            return Error.NotFound("Patient.NotFound", "Patient profile was not found.");

        if (patient.AccountId.HasValue)
        {
            await _authorizationServiceClient.DeletePatientAccountAsync(patient.AccountId.Value, cancellationToken);
        }

        _patientRepository.Remove(patient.Id);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Patient.ConcurrencyConflict", "Patient was modified by another operation. Please retry.");

        _logger.LogInformation(
            "Patient {PatientId} deleted along with account {AccountId}",
            patient.Id,
            patient.AccountId);

        return Result.Success;
    }
}
