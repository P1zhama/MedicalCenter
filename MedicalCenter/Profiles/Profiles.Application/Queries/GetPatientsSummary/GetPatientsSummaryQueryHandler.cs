using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetPatientsSummary;

public sealed class GetPatientsSummaryQueryHandler
    : IRequestHandler<GetPatientsSummaryQuery, ErrorOr<IReadOnlyList<PatientSummaryDto>>>
{
    private readonly IPatientQueryRepository _repository;

    public GetPatientsSummaryQueryHandler(IPatientQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<PatientSummaryDto>>> Handle(
        GetPatientsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var patients = await _repository.GetSummariesAsync(request.Ids, cancellationToken);

        return ErrorOrFactory.From(patients);
    }
}
