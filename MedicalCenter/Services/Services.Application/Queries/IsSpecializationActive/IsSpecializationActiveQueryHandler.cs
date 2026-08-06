using ErrorOr;
using MediatR;
using Services.Application.Common.Interfaces;

namespace Services.Application.Queries.IsSpecializationActive;

public sealed class IsSpecializationActiveQueryHandler : IRequestHandler<IsSpecializationActiveQuery, ErrorOr<bool>>
{
    private readonly ISpecializationQueryRepository _specializationQueryRepository;

    public IsSpecializationActiveQueryHandler(ISpecializationQueryRepository specializationQueryRepository)
    {
        _specializationQueryRepository = specializationQueryRepository;
    }

    public async Task<ErrorOr<bool>> Handle(IsSpecializationActiveQuery request, CancellationToken cancellationToken)
    {
        var isActive = await _specializationQueryRepository.IsActiveAsync(request.Id, cancellationToken);

        return isActive;
    }
}
