using MediatR;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Commands.CreateDoctor;

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, Guid>
{
    private readonly IAuthorizationServiceClient _authorizationServiceClient;
    private readonly IWorkerRepository _workerRepository;
    private readonly ILogger<CreateDoctorCommandHandler> _logger;

    public CreateDoctorCommandHandler(
        IAuthorizationServiceClient authorizationServiceClient,
        IWorkerRepository workerRepository,
        ILogger<CreateDoctorCommandHandler> logger)
    {
        _authorizationServiceClient = authorizationServiceClient;
        _workerRepository = workerRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        // 1. Синхронно создаём учётную запись в Authorization: там генерируется пароль
        // и уходит письмо с логином/паролем доктору (см. Authorization WorkerCreatedEventHandler)
        var accountId = await _authorizationServiceClient.CreateWorkerAccountAsync(
            request.Email, "Doctor", request.CreatedBy, cancellationToken);

        try
        {
            // 2. Создаём профиль сразу со всеми полями анкеты — без промежуточных "заглушек"
            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                DateOfBirth = request.DateOfBirth,
                SpecializationId = request.SpecializationId,
                OfficeId = request.OfficeId,
                CareerStartYear = request.CareerStartYear,
                Status = request.Status,
                PhotoUrl = request.PhotoUrl
            };

            await _workerRepository.AddDoctorAsync(doctor, cancellationToken);

            return doctor.Id;
        }
        catch (Exception ex)
        {
            // Компенсация: аккаунт в Authorization уже создан, а профиль сохранить не удалось.
            // Чтобы не оставлять осиротевший аккаунт, просим Authorization его удалить.
            // Откат — best-effort: если и он упадёт, хотя бы фиксируем это в логах
            _logger.LogError(ex, "Failed to save doctor profile after account {AccountId} was created. Compensating.", accountId);

            try
            {
                // CancellationToken.None: откат должен завершиться, даже если исходный запрос отменён
                await _authorizationServiceClient.DeleteWorkerAccountAsync(accountId, CancellationToken.None);
            }
            catch (Exception compensationEx)
            {
                _logger.LogError(compensationEx, "Compensation failed: account {AccountId} may be left orphaned.", accountId);
            }

            throw;
        }
    }
}
