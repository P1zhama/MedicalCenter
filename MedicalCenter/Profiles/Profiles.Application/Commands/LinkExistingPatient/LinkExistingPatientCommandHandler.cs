using Common.Abstractions.Eventing;
using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Commands.LinkExistingPatient;

public sealed class LinkExistingPatientCommandHandler
    : IRequestHandler<LinkExistingPatientCommand, ErrorOr<Success>>
{
    private readonly IPatientCommandRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public LinkExistingPatientCommandHandler(
        IPatientCommandRepository patientCommandRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _patientRepository = patientCommandRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(LinkExistingPatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.PatientId, cancellationToken);
        if (patient is null)
            return Error.NotFound("Patient.NotFound", "Patient profile was not found.");

        var now = _timeProvider.GetUtcNow();
        var expectedVersion = patient.Version;

        var linkResult = patient.LinkToAccount(request.AccountId, now);
        if (linkResult.IsError)
            return linkResult.Errors;

        _patientRepository.Update(patient, expectedVersion);

        await _eventPublisher.PublishAsync(
            new ProfileLinkedToAccountEvent(request.AccountId, patient.Id, now.UtcDateTime),
            cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Patient.ConcurrencyConflict", "Patient was modified by another operation. Please retry.");

        return Result.Success;
    }
}
