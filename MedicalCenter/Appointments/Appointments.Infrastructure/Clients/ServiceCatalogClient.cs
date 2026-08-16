using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Grpc.Core;
using Services.Api.Protos;

namespace Appointments.Infrastructure.Clients;

public sealed class ServiceCatalogClient : IServiceCatalogClient
{
    private readonly ServicesService.ServicesServiceClient _client;

    public ServiceCatalogClient(ServicesService.ServicesServiceClient client)
    {
        _client = client;
    }

    public async Task<ServiceForAppointmentDto?> GetServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetServiceForAppointmentAsync(
                new GetServiceForAppointmentRequest { ServiceId = serviceId.ToString() },
                cancellationToken: cancellationToken);

            return new ServiceForAppointmentDto(
                Guid.Parse(response.ServiceId),
                response.Name,
                Guid.Parse(response.SpecializationId),
                response.TimeSlotMinutes,
                response.IsActive);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
