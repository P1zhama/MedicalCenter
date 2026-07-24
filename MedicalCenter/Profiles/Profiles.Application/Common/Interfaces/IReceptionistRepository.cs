using Profiles.Domain;

namespace Profiles.Application.Common.Interfaces;

public interface IReceptionistRepository
{
    Task AddAsync(Receptionist receptionist, CancellationToken cancellationToken = default);
}
