using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.CreatePatientByReceptionist;

public sealed class CreatePatientByReceptionistCommandHandler
    : IRequestHandler<CreatePatientByReceptionistCommand, ErrorOr<Guid>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;

    public CreatePatientByReceptionistCommandHandler(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IGuidProvider guidProvider)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
    }

    public async Task<ErrorOr<Guid>> Handle(
        CreatePatientByReceptionistCommand request,
        CancellationToken cancellationToken)
    {
        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        var now = _timeProvider.GetUtcNow();

        var patientResult = Patient.Create(
            _guidProvider.NewGuid(),
            accountId: null,
            nameResult.Value,
            request.DateOfBirth,
            phoneNumber: null,
            photoUrl: null,
            createdBy: Guid.Empty,
            now);

        if (patientResult.IsError)
            return patientResult.Errors;

        await _patientRepository.AddAsync(patientResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return patientResult.Value.Id;
    }
}
