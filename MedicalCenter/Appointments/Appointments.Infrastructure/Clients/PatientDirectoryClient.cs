using Appointments.Application.Common.Interfaces;
using Profiles.Api.Protos;

namespace Appointments.Infrastructure.Clients;

public sealed class PatientDirectoryClient : IPatientDirectoryClient
{
    private readonly ProfilesService.ProfilesServiceClient _client;

    public PatientDirectoryClient(ProfilesService.ProfilesServiceClient client)
    {
        _client = client;
    }

    public async Task<bool> ExistsAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var response = await _client.PatientExistsAsync(
            new PatientExistsRequest { PatientId = patientId.ToString() },
            cancellationToken: cancellationToken);

        return response.Exists;
    }
}
