using ErrorOr;
using MediatR;
using Offices.Application.Common.Dtos;
using Offices.Application.Common.Interfaces;

namespace Offices.Application.Queries.GetOfficeById;

public sealed class GetOfficeByIdQueryHandler : IRequestHandler<GetOfficeByIdQuery, ErrorOr<OfficeDto>>
{
    private readonly IOfficeQueryRepository _officeQueryRepository;

    public GetOfficeByIdQueryHandler(IOfficeQueryRepository officeQueryRepository)
    {
        _officeQueryRepository = officeQueryRepository;
    }

    public async Task<ErrorOr<OfficeDto>> Handle(GetOfficeByIdQuery request, CancellationToken cancellationToken)
    {
        var office = await _officeQueryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (office is null)
            return Error.NotFound("Office.NotFound", "Office was not found.");

        return office;
    }
}
