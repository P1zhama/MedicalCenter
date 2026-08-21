using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetPatientById;

public sealed class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, ErrorOr<PatientDto>>
{
    private readonly IPatientQueryRepository _repository;
    private readonly IAppointmentServiceClient _appointmentServiceClient;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetPatientByIdQueryHandler(
        IPatientQueryRepository repository,
        IAppointmentServiceClient appointmentServiceClient,
        ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _appointmentServiceClient = appointmentServiceClient;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<PatientDto>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.User;

        if (user is not null && user.Roles.Contains(Roles.Doctor))
        {
            if (!user.HasProfile)
                return Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");

            var isTreatingDoctor = await _appointmentServiceClient.HasApprovedAppointmentAsync(
                user.ProfileId!.Value,
                request.Id,
                cancellationToken);

            if (!isTreatingDoctor)
                return Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");
        }

        var patient = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (patient is null)
            return Error.NotFound("Patient.NotFound", "Patient profile was not found.");

        return patient;
    }
}
