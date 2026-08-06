using Common.Abstractions.Eventing;
using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Domain.Services;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands;

public sealed class CreatePatientProfileCommandHandler
    : IRequestHandler<CreatePatientProfileCommand, ErrorOr<ProfileCreationResult>>
{
    private readonly IPatientCommandRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;

    public CreatePatientProfileCommandHandler(
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

    public async Task<ErrorOr<ProfileCreationResult>> Handle(
        CreatePatientProfileCommand request,
        CancellationToken cancellationToken)
    {
        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        var candidates = await _patientRepository.GetMatchCandidatesAsync(
            request.FirstName, request.LastName, cancellationToken);

        var match = PatientMatcher.FindBestMatch(nameResult.Value, request.DateOfBirth, candidates);
        if (match is not null)
        {
            var matchedInfo = new MatchedProfileDto(
                match.Name.FirstName,
                match.Name.LastName,
                match.Name.MiddleName,
                match.DateOfBirth);

            return new ProfileCreationResult(true, match.Id, matchedInfo, null);
        }

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

        return new ProfileCreationResult(false, null, null, patientResult.Value.Id);
    }
}
