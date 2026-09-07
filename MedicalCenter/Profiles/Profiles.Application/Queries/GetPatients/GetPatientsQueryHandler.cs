using Common.Abstractions.Paging;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetPatients;

public sealed class GetPatientsQueryHandler
    : IRequestHandler<GetPatientsQuery, ErrorOr<PagedResult<PatientListItemDto>>>
{
    private readonly IPatientQueryRepository _repository;

    public GetPatientsQueryHandler(IPatientQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<PagedResult<PatientListItemDto>>> Handle(
        GetPatientsQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize) = PageRequest.Normalize(request.Page, request.PageSize);

        var patients = await _repository.SearchAsync(request.Search, page, pageSize, cancellationToken);

        return patients;
    }
}
