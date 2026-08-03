using ErrorOr;
using MediatR;
using Offices.Application.Common.Interfaces;

namespace Offices.Application.Queries.IsOfficeActive;

public sealed class IsOfficeActiveQueryHandler : IRequestHandler<IsOfficeActiveQuery, ErrorOr<bool>>
{
    private readonly IOfficeQueryRepository _officeQueryRepository;

    public IsOfficeActiveQueryHandler(IOfficeQueryRepository officeQueryRepository)
    {
        _officeQueryRepository = officeQueryRepository;
    }

    public async Task<ErrorOr<bool>> Handle(IsOfficeActiveQuery request, CancellationToken cancellationToken)
    {
        var isActive = await _officeQueryRepository.IsActiveAsync(request.Id, cancellationToken);

        return isActive;
    }
}
