using Common.Abstractions.Eventing;
using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.CreateDoctor;

public sealed class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, ErrorOr<Guid>>
{
    private const string DoctorRole = "Doctor";

    private readonly IAuthorizationServiceClient _authorizationServiceClient;
    private readonly IOfficeServiceClient _officeServiceClient;
    private readonly ISpecializationServiceClient _specializationServiceClient;
    private readonly IDoctorCommandRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<CreateDoctorCommandHandler> _logger;

    public CreateDoctorCommandHandler(
        IAuthorizationServiceClient authorizationServiceClient,
        IOfficeServiceClient officeServiceClient,
        ISpecializationServiceClient specializationServiceClient,
        IDoctorCommandRepository doctorCommandRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<CreateDoctorCommandHandler> logger)
    {
        _authorizationServiceClient = authorizationServiceClient;
        _officeServiceClient = officeServiceClient;
        _specializationServiceClient = specializationServiceClient;
        _doctorRepository = doctorCommandRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        if (!await _officeServiceClient.IsOfficeActiveAsync(request.OfficeId, cancellationToken))
            return Error.Validation("Doctor.OfficeId", "Please, choose the office");

        if (!await _specializationServiceClient.IsSpecializationActiveAsync(request.SpecializationId, cancellationToken))
            return Error.Validation("Doctor.SpecializationId", "Please, choose the specialisation");

        var now = _timeProvider.GetUtcNow();
        var createdBy = _currentUserProvider.User?.Id ?? Guid.Empty;

        var accountId = await _authorizationServiceClient.CreateWorkerAccountAsync(
            request.Email, DoctorRole, createdBy, cancellationToken);

        var doctorResult = Doctor.Create(
            _guidProvider.NewGuid(),
            accountId,
            nameResult.Value,
            request.DateOfBirth,
            request.SpecializationId,
            request.OfficeId,
            request.CareerStartYear,
            request.Status,
            request.PhotoUrl,
            createdBy,
            now);

        if (doctorResult.IsError)
        {
            await CompensateAsync(accountId);
            return doctorResult.Errors;
        }

        try
        {
            await _doctorRepository.AddAsync(doctorResult.Value, cancellationToken);

            await _eventPublisher.PublishAsync(
                new ProfileLinkedToAccountEvent(accountId, doctorResult.Value.Id, now.UtcDateTime),
                cancellationToken);

            if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            {
                await CompensateAsync(accountId);

                return Error.Conflict("Doctor.ConcurrencyConflict", "Doctor was modified by another operation. Please retry.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to persist doctor profile after account {AccountId} was created. Compensating.",
                accountId);

            await CompensateAsync(accountId);

            throw;
        }

        _logger.LogInformation("Doctor {DoctorId} created for account {AccountId}", doctorResult.Value.Id, accountId);

        return doctorResult.Value.Id;
    }

    private async Task CompensateAsync(Guid accountId)
    {
        try
        {
            await _authorizationServiceClient.DeleteWorkerAccountAsync(accountId, CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Compensation failed: account {AccountId} may be left orphaned.", accountId);
        }
    }
}
