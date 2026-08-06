using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetReceptionists;

public sealed class GetReceptionistsQueryHandler
    : IRequestHandler<GetReceptionistsQuery, ErrorOr<IReadOnlyList<ReceptionistListItemDto>>>
{
    private readonly IReceptionistQueryRepository _repository;

    public GetReceptionistsQueryHandler(IReceptionistQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<ReceptionistListItemDto>>> Handle(
        GetReceptionistsQuery request,
        CancellationToken cancellationToken)
    {
        var receptionists = await _repository.GetAllAsync(cancellationToken);

        return ErrorOrFactory.From(receptionists);
    }
}
