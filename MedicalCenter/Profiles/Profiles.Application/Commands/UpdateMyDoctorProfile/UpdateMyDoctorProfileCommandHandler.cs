using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.UpdateMyDoctorProfile;

public sealed class UpdateMyDoctorProfileCommandHandler
    : IRequestHandler<UpdateMyDoctorProfileCommand, ErrorOr<Success>>
{
    private readonly IDoctorCommandRepository _doctorRepository;
    private readonly IDoctorQueryRepository _doctorQueryRepository;
    private readonly IOfficeServiceClient _officeServiceClient;
    private readonly ISpecializationServiceClient _specializationServiceClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public UpdateMyDoctorProfileCommandHandler(
        IDoctorCommandRepository doctorCommandRepository,
        IDoctorQueryRepository doctorQueryRepository,
        IOfficeServiceClient officeServiceClient,
        ISpecializationServiceClient specializationServiceClient,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _doctorRepository = doctorCommandRepository;
        _doctorQueryRepository = doctorQueryRepository;
        _officeServiceClient = officeServiceClient;
        _specializationServiceClient = specializationServiceClient;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateMyDoctorProfileCommand request,
        CancellationToken cancellationToken)
    {
        var accountId = _currentUserProvider.User?.Id;
        if (accountId is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var now = _timeProvider.GetUtcNow();

        var own = await _doctorQueryRepository.GetByAccountIdAsync(accountId.Value, now.Year, cancellationToken);
        if (own is null)
            return Error.NotFound("Doctor.NotFound", "Doctor profile was not found.");

        var doctor = await _doctorRepository.GetByIdAsync(own.Id, cancellationToken);
        if (doctor is null)
            return Error.NotFound("Doctor.NotFound", "Doctor profile was not found.");

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

        var expectedVersion = doctor.Version;

        var updateResult = doctor.Update(
            nameResult.Value,
            request.DateOfBirth,
            request.SpecializationId,
            request.OfficeId,
            request.CareerStartYear,
            doctor.Status,
            request.PhotoUrl,
            accountId.Value,
            now);

        if (updateResult.IsError)
            return updateResult.Errors;

        _doctorRepository.Update(doctor, expectedVersion);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Doctor.ConcurrencyConflict", "Doctor was modified by another operation. Please retry.");

        return Result.Success;
    }
}
