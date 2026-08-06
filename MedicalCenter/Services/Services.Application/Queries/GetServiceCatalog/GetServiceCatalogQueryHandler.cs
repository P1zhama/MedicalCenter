using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;

namespace Services.Application.Queries.GetServiceCatalog;

public sealed class GetServiceCatalogQueryHandler : IRequestHandler<GetServiceCatalogQuery, ErrorOr<ServiceCatalogDto>>
{
    private readonly IServiceQueryRepository _repository;

    public GetServiceCatalogQueryHandler(IServiceQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<ServiceCatalogDto>> Handle(
        GetServiceCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var catalog = await _repository.GetActiveCatalogAsync(cancellationToken);

        return catalog;
    }
}
