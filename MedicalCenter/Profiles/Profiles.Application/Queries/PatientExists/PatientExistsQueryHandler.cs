using ErrorOr;
using MediatR;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.PatientExists;

public sealed class PatientExistsQueryHandler : IRequestHandler<PatientExistsQuery, ErrorOr<bool>>
{
    private readonly IPatientQueryRepository _repository;

    public PatientExistsQueryHandler(IPatientQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<bool>> Handle(PatientExistsQuery request, CancellationToken cancellationToken)
        => await _repository.ExistsAsync(request.Id, cancellationToken);
}
