using Appointments.Api.Protos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Infrastructure.Services;

public sealed class AppointmentServiceClient : IAppointmentServiceClient
{
    private readonly AppointmentsService.AppointmentsServiceClient _client;

    public AppointmentServiceClient(AppointmentsService.AppointmentsServiceClient client)
    {
        _client = client;
    }

    public async Task<bool> HasApprovedAppointmentAsync(
        Guid doctorId,
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.HasApprovedAppointmentAsync(
            new HasApprovedAppointmentRequest
            {
                DoctorId = doctorId.ToString(),
                PatientId = patientId.ToString()
            },
            cancellationToken: cancellationToken);

        return response.HasApproved;
    }
}
