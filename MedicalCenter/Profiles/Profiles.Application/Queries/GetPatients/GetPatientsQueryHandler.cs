using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetPatients;

public sealed class GetPatientsQueryHandler
    : IRequestHandler<GetPatientsQuery, ErrorOr<IReadOnlyList<PatientListItemDto>>>
{
    private readonly IPatientQueryRepository _repository;

    public GetPatientsQueryHandler(IPatientQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<PatientListItemDto>>> Handle(
        GetPatientsQuery request,
        CancellationToken cancellationToken)
    {
        var patients = await _repository.SearchAsync(request.Search, cancellationToken);

        return ErrorOrFactory.From(patients);
    }
}
