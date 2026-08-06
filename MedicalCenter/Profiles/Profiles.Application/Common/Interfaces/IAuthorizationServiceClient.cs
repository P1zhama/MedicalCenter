using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Common.Interfaces;

public interface IAuthorizationServiceClient
{
    Task<Guid> CreateWorkerAccountAsync(string email, string roleName, Guid createdBy, CancellationToken cancellationToken);

    Task DeleteWorkerAccountAsync(Guid accountId, CancellationToken cancellationToken);

    Task DeletePatientAccountAsync(Guid accountId, CancellationToken cancellationToken);
}
