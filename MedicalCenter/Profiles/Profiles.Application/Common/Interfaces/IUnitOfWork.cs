namespace Profiles.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken = default);
}
