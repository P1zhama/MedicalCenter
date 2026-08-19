using Appointments.Application.Common.Dtos;
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

    public async Task<IReadOnlyList<PatientSummaryDto>> GetSummariesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        var request = new GetPatientsSummaryRequest();
        request.PatientIds.AddRange(ids.Select(id => id.ToString()));

        var response = await _client.GetPatientsSummaryAsync(request, cancellationToken: cancellationToken);

        return response.Patients
            .Select(patient => new PatientSummaryDto(
                Guid.Parse(patient.PatientId),
                patient.FirstName,
                patient.LastName,
                string.IsNullOrEmpty(patient.MiddleName) ? null : patient.MiddleName,
                string.IsNullOrEmpty(patient.PhoneNumber) ? null : patient.PhoneNumber))
            .ToList();
    }

    public async Task<bool> ExistsAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var response = await _client.PatientExistsAsync(
            new PatientExistsRequest { PatientId = patientId.ToString() },
            cancellationToken: cancellationToken);

        return response.Exists;
    }
}
