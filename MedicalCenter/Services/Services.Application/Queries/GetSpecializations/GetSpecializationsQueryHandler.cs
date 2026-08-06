using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;

namespace Services.Application.Queries.GetSpecializations;

public sealed class GetSpecializationsQueryHandler
    : IRequestHandler<GetSpecializationsQuery, ErrorOr<IReadOnlyList<SpecializationListItemDto>>>
{
    private readonly ISpecializationQueryRepository _repository;

    public GetSpecializationsQueryHandler(ISpecializationQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<SpecializationListItemDto>>> Handle(
        GetSpecializationsQuery request,
        CancellationToken cancellationToken)
    {
        var specializations = await _repository.GetAllAsync(cancellationToken);

        return ErrorOrFactory.From(specializations);
    }
}
