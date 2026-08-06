using Profiles.Application.Common.Interfaces;
using Services.Api.Protos;

namespace Profiles.Infrastructure.Services;

public sealed class SpecializationServiceClient : ISpecializationServiceClient
{
    private readonly ServicesService.ServicesServiceClient _client;

    public SpecializationServiceClient(ServicesService.ServicesServiceClient client)
    {
        _client = client;
    }

    public async Task<bool> IsSpecializationActiveAsync(
        Guid specializationId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.IsSpecializationActiveAsync(
            new IsSpecializationActiveRequest { SpecializationId = specializationId.ToString() },
            cancellationToken: cancellationToken);

        return response.IsActive;
    }
}
