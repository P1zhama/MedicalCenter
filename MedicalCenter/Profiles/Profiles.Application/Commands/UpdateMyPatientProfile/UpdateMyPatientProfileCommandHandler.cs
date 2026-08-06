using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.UpdateMyPatientProfile;

public sealed class UpdateMyPatientProfileCommandHandler
    : IRequestHandler<UpdateMyPatientProfileCommand, ErrorOr<Success>>
{
    private readonly IPatientCommandRepository _patientRepository;
    private readonly IPatientQueryRepository _patientQueryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public UpdateMyPatientProfileCommandHandler(
        IPatientCommandRepository patientCommandRepository,
        IPatientQueryRepository patientQueryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _patientRepository = patientCommandRepository;
        _patientQueryRepository = patientQueryRepository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateMyPatientProfileCommand request,
        CancellationToken cancellationToken)
    {
        var accountId = _currentUserProvider.User?.Id;
        if (accountId is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var own = await _patientQueryRepository.GetByAccountIdAsync(accountId.Value, cancellationToken);
        if (own is null)
            return Error.NotFound("Patient.NotFound", "Patient profile was not found.");

        var patient = await _patientRepository.GetByIdAsync(own.Id, cancellationToken);
        if (patient is null)
            return Error.NotFound("Patient.NotFound", "Patient profile was not found.");

        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        var now = _timeProvider.GetUtcNow();
        var expectedVersion = patient.Version;

        var updateResult = patient.Update(
            nameResult.Value,
            request.DateOfBirth,
            request.PhoneNumber,
            request.PhotoUrl,
            accountId.Value,
            now);

        if (updateResult.IsError)
            return updateResult.Errors;

        _patientRepository.Update(patient, expectedVersion);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Patient.ConcurrencyConflict", "Patient was modified by another operation. Please retry.");

        return Result.Success;
    }
}
