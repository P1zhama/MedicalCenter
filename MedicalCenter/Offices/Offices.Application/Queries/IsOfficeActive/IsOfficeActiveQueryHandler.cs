using MediatR;
using Offices.Application.Common.Interfaces;

namespace Offices.Application.Queries.IsOfficeActive;

public sealed class IsOfficeActiveQueryHandler : IRequestHandler<IsOfficeActiveQuery, bool>
{
    private readonly IOfficeQueryRepository _officeQueryRepository;

    public IsOfficeActiveQueryHandler(IOfficeQueryRepository officeQueryRepository)
    {
        _officeQueryRepository = officeQueryRepository;
    }

    public Task<bool> Handle(IsOfficeActiveQuery request, CancellationToken cancellationToken)
        => _officeQueryRepository.IsActiveAsync(request.Id, cancellationToken);
}
