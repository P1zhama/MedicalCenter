using MediatR;
using Offices.Application.Common.Dtos;
using Offices.Application.Common.Interfaces;
using Offices.Application.Common.Mappings;


namespace Offices.Application.Queries.GetOfficeById;

public class GetOfficeByIdQueryHandler : IRequestHandler<GetOfficeByIdQuery, OfficeDto>
{
    private readonly IOfficeRepository _repository;

    public GetOfficeByIdQueryHandler(IOfficeRepository repository)
    {
        _repository = repository;
    }

    public async Task<OfficeDto> Handle(GetOfficeByIdQuery request, CancellationToken cancellationToken)
    {
        var office = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Office {request.Id} was not found.");

        return office.ToDto();
    }
}
