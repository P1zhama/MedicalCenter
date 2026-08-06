using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetMyPatientProfile;

public sealed class GetMyPatientProfileQueryHandler : IRequestHandler<GetMyPatientProfileQuery, ErrorOr<PatientDto>>
{
    private readonly IPatientQueryRepository _repository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetMyPatientProfileQueryHandler(
        IPatientQueryRepository repository,
        ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<PatientDto>> Handle(
        GetMyPatientProfileQuery request,
        CancellationToken cancellationToken)
    {
        var accountId = _currentUserProvider.User?.Id;
        if (accountId is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var patient = await _repository.GetByAccountIdAsync(accountId.Value, cancellationToken);
        if (patient is null)
            return Error.NotFound("Patient.NotFound", "Patient profile was not found.");

        return patient;
    }
}
