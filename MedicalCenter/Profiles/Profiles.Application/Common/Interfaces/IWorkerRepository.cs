using Profiles.Domain;

namespace Profiles.Application.Common.Interfaces;

public interface IWorkerRepository
{
    Task AddReceptionistAsync(Receptionist receptionist, CancellationToken cancellationToken);
}
