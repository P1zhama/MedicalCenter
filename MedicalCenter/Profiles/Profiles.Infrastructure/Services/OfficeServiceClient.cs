using Offices.Api.Protos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Infrastructure.Services;

public sealed class OfficeServiceClient : IOfficeServiceClient
{
    private readonly OfficesService.OfficesServiceClient _client;

    public OfficeServiceClient(OfficesService.OfficesServiceClient client)
    {
        _client = client;
    }

    public async Task<bool> IsOfficeActiveAsync(Guid officeId, CancellationToken cancellationToken = default)
    {
        var response = await _client.IsOfficeActiveAsync(
            new IsOfficeActiveRequest { OfficeId = officeId.ToString() },
            cancellationToken: cancellationToken);

        return response.IsActive;
    }
}
