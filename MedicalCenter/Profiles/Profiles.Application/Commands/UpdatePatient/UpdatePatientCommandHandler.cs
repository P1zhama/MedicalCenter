using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.UpdatePatient;

public sealed class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, ErrorOr<Success>>
{
    private readonly IPatientCommandRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public UpdatePatientCommandHandler(
        IPatientCommandRepository patientCommandRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _patientRepository = patientCommandRepository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.Id, cancellationToken);
        if (patient is null)
            return Error.NotFound("Patient.NotFound", "Patient profile was not found.");

        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        var now = _timeProvider.GetUtcNow();
        var updatedBy = _currentUserProvider.User?.Id ?? Guid.Empty;
        var expectedVersion = patient.Version;

        var updateResult = patient.Update(
            nameResult.Value,
            request.DateOfBirth,
            request.PhoneNumber,
            request.PhotoUrl,
            updatedBy,
            now);

        if (updateResult.IsError)
            return updateResult.Errors;

        _patientRepository.Update(patient, expectedVersion);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Patient.ConcurrencyConflict", "Patient was modified by another operation. Please retry.");

        return Result.Success;
    }
}
