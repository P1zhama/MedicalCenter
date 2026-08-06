namespace Profiles.Application.Common.Interfaces;

public interface IOfficeServiceClient
{
    Task<bool> IsOfficeActiveAsync(Guid officeId, CancellationToken cancellationToken = default);
}
