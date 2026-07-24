using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.CreateDoctor;

public sealed class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, ErrorOr<Guid>>
{
    private const string DoctorRole = "Doctor";

    private readonly IAuthorizationServiceClient _authorizationServiceClient;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<CreateDoctorCommandHandler> _logger;

    public CreateDoctorCommandHandler(
        IAuthorizationServiceClient authorizationServiceClient,
        IDoctorRepository doctorRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<CreateDoctorCommandHandler> logger)
    {
        _authorizationServiceClient = authorizationServiceClient;
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        Guid.TryParse(request.CreatedBy, out var createdBy);
        var now = _timeProvider.GetUtcNow();

        var accountId = await _authorizationServiceClient.CreateWorkerAccountAsync(
            request.Email, DoctorRole, request.CreatedBy, cancellationToken);

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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
