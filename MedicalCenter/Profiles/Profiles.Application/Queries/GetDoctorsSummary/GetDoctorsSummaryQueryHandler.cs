using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetDoctorsSummary;

public sealed class GetDoctorsSummaryQueryHandler
    : IRequestHandler<GetDoctorsSummaryQuery, ErrorOr<IReadOnlyList<DoctorSummaryDto>>>
{
    private readonly IDoctorQueryRepository _repository;

    public GetDoctorsSummaryQueryHandler(IDoctorQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<DoctorSummaryDto>>> Handle(
        GetDoctorsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var doctors = await _repository.GetSummariesAsync(request.Ids, cancellationToken);

        return ErrorOrFactory.From(doctors);
    }
}
