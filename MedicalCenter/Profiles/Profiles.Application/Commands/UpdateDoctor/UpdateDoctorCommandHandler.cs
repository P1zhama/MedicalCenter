using Common.Abstractions.Eventing;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Interfaces;
using Profiles.Application.Common.Services;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.UpdateDoctor;

public sealed class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand, ErrorOr<Success>>
{
    private readonly IDoctorCommandRepository _doctorRepository;
    private readonly IOfficeServiceClient _officeServiceClient;
    private readonly ISpecializationServiceClient _specializationServiceClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public UpdateDoctorCommandHandler(
        IDoctorCommandRepository doctorCommandRepository,
        IOfficeServiceClient officeServiceClient,
        ISpecializationServiceClient specializationServiceClient,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _doctorRepository = doctorCommandRepository;
        _officeServiceClient = officeServiceClient;
        _specializationServiceClient = specializationServiceClient;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.Id, cancellationToken);
        if (doctor is null)
            return Error.NotFound("Doctor.NotFound", "Doctor was not found.");

        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        if (doctor.OfficeId != request.OfficeId
            && !await _officeServiceClient.IsOfficeActiveAsync(request.OfficeId, cancellationToken))
        {
            return Error.Validation("Doctor.OfficeId", "Please, choose the office");
        }

        if (doctor.SpecializationId != request.SpecializationId
            && !await _specializationServiceClient.IsSpecializationActiveAsync(request.SpecializationId, cancellationToken))
        {
            return Error.Validation("Doctor.SpecializationId", "Please, choose the specialisation");
        }

        var now = _timeProvider.GetUtcNow();
        var updatedBy = _currentUserProvider.User?.Id ?? Guid.Empty;
        var expectedVersion = doctor.Version;

        var updateResult = doctor.Update(
            nameResult.Value,
            request.DateOfBirth,
            request.SpecializationId,
            request.OfficeId,
            request.CareerStartYear,
            request.Status,
            request.PhotoUrl,
            updatedBy,
            now);

        if (updateResult.IsError)
            return updateResult.Errors;

        _doctorRepository.Update(doctor, expectedVersion);

        var integrationEvent = WorkerStatusEvents.ForTransition(updateResult.Value, doctor.AccountId, now);
        if (integrationEvent is not null)
            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Doctor.ConcurrencyConflict", "Doctor was modified by another operation. Please retry.");

        return Result.Success;
    }
}
