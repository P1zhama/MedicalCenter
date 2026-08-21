using Appointments.Application.Common.Interfaces;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.HasApprovedAppointment;

public sealed class HasApprovedAppointmentQueryHandler
    : IRequestHandler<HasApprovedAppointmentQuery, ErrorOr<bool>>
{
    private readonly IAppointmentQueryRepository _repository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public HasApprovedAppointmentQueryHandler(
        IAppointmentQueryRepository repository,
        ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<bool>> Handle(
        HasApprovedAppointmentQuery request,
        CancellationToken cancellationToken)
    {
        var profileId = _currentUserProvider.User?.ProfileId;

        if (profileId != request.DoctorId)
            return Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");

        var hasApproved = await _repository.HasApprovedWithPatientAsync(
            request.DoctorId,
            request.PatientId,
            cancellationToken);

        return hasApproved;
    }
}
