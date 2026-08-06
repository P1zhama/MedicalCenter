using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;

namespace Services.Application.Queries.GetServiceForAppointment;

public sealed class GetServiceForAppointmentQueryHandler
    : IRequestHandler<GetServiceForAppointmentQuery, ErrorOr<ServiceForAppointmentDto>>
{
    private readonly IServiceQueryRepository _repository;

    public GetServiceForAppointmentQueryHandler(IServiceQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<ServiceForAppointmentDto>> Handle(
        GetServiceForAppointmentQuery request,
        CancellationToken cancellationToken)
    {
        var service = await _repository.GetForAppointmentAsync(request.Id, cancellationToken);
        if (service is null)
            return Error.NotFound("Service.NotFound", "Service was not found.");

        return service;
    }
}
