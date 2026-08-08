using ErrorOr;
using MediatR;
using Offices.Application.Common.Dtos;
using Offices.Application.Common.Interfaces;

namespace Offices.Application.Queries.GetActiveOffices;

public sealed class GetActiveOfficesQueryHandler
    : IRequestHandler<GetActiveOfficesQuery, ErrorOr<IReadOnlyList<PublicOfficeDto>>>
{
    private readonly IOfficeQueryRepository _repository;

    public GetActiveOfficesQueryHandler(IOfficeQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<PublicOfficeDto>>> Handle(
        GetActiveOfficesQuery request,
        CancellationToken cancellationToken)
    {
        var offices = await _repository.GetActiveAsync(cancellationToken);

        return ErrorOrFactory.From(offices);
    }
}
