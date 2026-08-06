using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetPatientById;

public sealed class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, ErrorOr<PatientDto>>
{
    private readonly IPatientQueryRepository _repository;

    public GetPatientByIdQueryHandler(IPatientQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<PatientDto>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (patient is null)
            return Error.NotFound("Patient.NotFound", "Patient profile was not found.");

        return patient;
    }
}
