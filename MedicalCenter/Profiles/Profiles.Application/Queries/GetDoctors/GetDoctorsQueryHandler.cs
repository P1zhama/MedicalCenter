using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetDoctors;

public sealed class GetDoctorsQueryHandler
    : IRequestHandler<GetDoctorsQuery, ErrorOr<IReadOnlyList<DoctorListItemDto>>>
{
    private readonly IDoctorQueryRepository _repository;

    public GetDoctorsQueryHandler(IDoctorQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<DoctorListItemDto>>> Handle(
        GetDoctorsQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new DoctorFilter(request.Search, request.SpecializationId, request.OfficeId);

        var doctors = await _repository.SearchAsync(filter, cancellationToken);

        return ErrorOrFactory.From(doctors);
    }
}
