namespace Profiles.Application.Common.Interfaces;

public interface ISpecializationServiceClient
{
    Task<bool> IsSpecializationActiveAsync(Guid specializationId, CancellationToken cancellationToken = default);
}
