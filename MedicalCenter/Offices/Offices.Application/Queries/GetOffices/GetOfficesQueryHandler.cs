using ErrorOr;
using MediatR;
using Offices.Application.Common.Dtos;
using Offices.Application.Common.Interfaces;

namespace Offices.Application.Queries.GetOffices;

public class GetOfficesQueryHandler : IRequestHandler<GetOfficesQuery, ErrorOr<IReadOnlyList<OfficeListItemDto>>>
{
    private readonly IOfficeQueryRepository _repository;

    public GetOfficesQueryHandler(IOfficeQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<OfficeListItemDto>>> Handle(GetOfficesQuery request, CancellationToken cancellationToken)
    {
        var offices = await _repository.GetAllAsync(cancellationToken);

        return ErrorOrFactory.From(offices);
    }
}
