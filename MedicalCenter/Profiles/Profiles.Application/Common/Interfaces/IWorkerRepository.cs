

using Profiles.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Common.Interfaces;

public interface IWorkerRepository
{
    Task AddDoctorAsync(Doctor doctor, CancellationToken cancellationToken);
    Task AddReceptionistAsync(Receptionist receptionist, CancellationToken cancellationToken);

    Task<int> DeactivateDoctorsByOfficeAsync(Guid officeId, CancellationToken cancellationToken);
}