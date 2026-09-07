using Common.Abstractions.Paging;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetDoctors;

public sealed class GetDoctorsQueryHandler
    : IRequestHandler<GetDoctorsQuery, ErrorOr<PagedResult<DoctorListItemDto>>>
{
    private readonly IDoctorQueryRepository _repository;

    public GetDoctorsQueryHandler(IDoctorQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<PagedResult<DoctorListItemDto>>> Handle(
        GetDoctorsQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new DoctorFilter(request.Search, request.SpecializationId, request.OfficeId);

        var (page, pageSize) = PageRequest.Normalize(request.Page, request.PageSize);

        return await _repository.SearchAsync(filter, page, pageSize, cancellationToken);
    }
}
