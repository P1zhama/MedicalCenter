

using Profiles.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Common.Interfaces;

public interface IWorkerRepository
{
    Task AddDoctorAsync(Doctor doctor, CancellationToken cancellationToken);
    Task AddReceptionistAsync(Receptionist receptionist, CancellationToken cancellationToken);

    // US-32 AC-2: перевести всех докторов офиса в статус Inactive.
    // Возвращает число затронутых докторов. Вызывается консьюмером OfficeDeactivatedEvent
    Task<int> DeactivateDoctorsByOfficeAsync(Guid officeId, CancellationToken cancellationToken);
}