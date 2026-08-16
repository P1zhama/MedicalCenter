using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Grpc.Core;
using Profiles.Api.Protos;

namespace Appointments.Infrastructure.Clients;

public sealed class DoctorDirectoryClient : IDoctorDirectoryClient
{
    private readonly ProfilesService.ProfilesServiceClient _client;

    public DoctorDirectoryClient(ProfilesService.ProfilesServiceClient client)
    {
        _client = client;
    }

    public async Task<DoctorForAppointmentDto?> GetDoctorAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetDoctorForAppointmentAsync(
                new GetDoctorForAppointmentRequest { DoctorId = doctorId.ToString() },
                cancellationToken: cancellationToken);

            return new DoctorForAppointmentDto(
                Guid.Parse(response.DoctorId),
                Guid.Parse(response.SpecializationId),
                Guid.Parse(response.OfficeId),
                response.IsAtWork);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<Guid>> GetAtWorkDoctorIdsAsync(
        Guid specializationId,
        Guid? officeId,
        CancellationToken cancellationToken = default)
    {
        var request = new GetDoctorsForAppointmentRequest
        {
            SpecializationId = specializationId.ToString(),
            OfficeId = officeId?.ToString() ?? string.Empty
        };

        var response = await _client.GetDoctorsForAppointmentAsync(request, cancellationToken: cancellationToken);

        return response.DoctorIds.Select(Guid.Parse).ToList();
    }
}
