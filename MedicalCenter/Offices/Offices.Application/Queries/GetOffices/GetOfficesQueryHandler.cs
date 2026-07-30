using MediatR;
using Offices.Application.Common.Dtos;
using Offices.Application.Common.Interfaces;

namespace Offices.Application.Queries.GetOffices;

public class GetOfficesQueryHandler : IRequestHandler<GetOfficesQuery, IReadOnlyList<OfficeListItemDto>>
{
    private readonly IOfficeQueryRepository _repository;

    public GetOfficesQueryHandler(IOfficeQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<OfficeListItemDto>> Handle(GetOfficesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}
