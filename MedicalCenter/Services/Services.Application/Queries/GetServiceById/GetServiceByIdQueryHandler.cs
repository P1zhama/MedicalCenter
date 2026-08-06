using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;

namespace Services.Application.Queries.GetServiceById;

public sealed class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ErrorOr<ServiceDto>>
{
    private readonly IServiceQueryRepository _repository;

    public GetServiceByIdQueryHandler(IServiceQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<ServiceDto>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (service is null)
            return Error.NotFound("Service.NotFound", "Service was not found.");

        return service;
    }
}
