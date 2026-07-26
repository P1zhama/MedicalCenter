using ErrorOr;
using MediatR;
using Offices.Application.Common.Dtos;
using Offices.Application.Common.Interfaces;
using Offices.Application.Common.Mappings;

namespace Offices.Application.Queries.GetOfficeById;

public sealed class GetOfficeByIdQueryHandler : IRequestHandler<GetOfficeByIdQuery, ErrorOr<OfficeDto>>
{
    private readonly IOfficeRepository _officeRepository;

    public GetOfficeByIdQueryHandler(IOfficeRepository officeRepository)
    {
        _officeRepository = officeRepository;
    }

    public async Task<ErrorOr<OfficeDto>> Handle(GetOfficeByIdQuery request, CancellationToken cancellationToken)
    {
        var office = await _officeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (office is null)
            return Error.NotFound("Office.NotFound", "Office was not found.");

        return office.ToDto();
    }
}
