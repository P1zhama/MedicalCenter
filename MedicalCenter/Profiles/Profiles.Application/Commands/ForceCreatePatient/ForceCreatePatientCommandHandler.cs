using Common.Abstractions.Eventing;
using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.ForceCreatePatient;

public sealed class ForceCreatePatientCommandHandler
    : IRequestHandler<ForceCreatePatientCommand, ErrorOr<Guid>>
{
    private readonly IPatientCommandRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;

    public ForceCreatePatientCommandHandler(
        IPatientCommandRepository patientCommandRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        TimeProvider timeProvider,
        IGuidProvider guidProvider)
    {
        _patientRepository = patientCommandRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
    }

    public async Task<ErrorOr<Guid>> Handle(ForceCreatePatientCommand request, CancellationToken cancellationToken)
    {
        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        var now = _timeProvider.GetUtcNow();

        var patientResult = Patient.Create(
            _guidProvider.NewGuid(),
            request.AccountId,
            nameResult.Value,
            request.DateOfBirth,
            request.PhoneNumber,
            request.PhotoUrl,
            createdBy: request.AccountId,
            now);

        if (patientResult.IsError)
            return patientResult.Errors;

        await _patientRepository.AddAsync(patientResult.Value, cancellationToken);

        await _eventPublisher.PublishAsync(
            new ProfileLinkedToAccountEvent(request.AccountId, patientResult.Value.Id, now.UtcDateTime),
            cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Patient.ConcurrencyConflict", "Patient was modified by another operation. Please retry.");

        return patientResult.Value.Id;
    }
}
