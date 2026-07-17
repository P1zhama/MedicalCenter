using MediatR;
using Offices.Application.Common.Dtos;
using Offices.Application.Common.Interfaces;
using Offices.Application.Common.Mappings;

namespace Offices.Application.Queries.GetOffices;

public class GetOfficesQueryHandler : IRequestHandler<GetOfficesQuery, IReadOnlyList<OfficeListItemDto>>
{
    private readonly IOfficeRepository _repository;

    public GetOfficesQueryHandler(IOfficeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<OfficeListItemDto>> Handle(GetOfficesQuery request, CancellationToken cancellationToken)
    {
        var offices = await _repository.GetAllAsync(cancellationToken);

        return offices.Select(o => o.ToListItem()).ToList();
    }
}
