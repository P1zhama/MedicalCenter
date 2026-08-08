using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;

namespace Services.Application.Queries.GetActiveSpecializations;

public sealed class GetActiveSpecializationsQueryHandler
    : IRequestHandler<GetActiveSpecializationsQuery, ErrorOr<IReadOnlyList<PublicSpecializationDto>>>
{
    private readonly ISpecializationQueryRepository _repository;

    public GetActiveSpecializationsQueryHandler(ISpecializationQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<PublicSpecializationDto>>> Handle(
        GetActiveSpecializationsQuery request,
        CancellationToken cancellationToken)
    {
        var specializations = await _repository.GetActiveAsync(cancellationToken);

        return ErrorOrFactory.From(specializations);
    }
}
