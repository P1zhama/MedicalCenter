using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using Common.Abstractions.Security;
using Common.Abstractions.Paging;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetPatientAppointments;

public sealed class GetPatientAppointmentsQueryHandler
    : IRequestHandler<GetPatientAppointmentsQuery, ErrorOr<PagedResult<AppointmentListItemDto>>>
{
    private readonly IAppointmentQueryRepository _repository;
    private readonly AppointmentDetailsResolver _detailsResolver;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetPatientAppointmentsQueryHandler(
        IAppointmentQueryRepository repository,
        AppointmentDetailsResolver detailsResolver,
        ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _detailsResolver = detailsResolver;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<PagedResult<AppointmentListItemDto>>> Handle(
        GetPatientAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var doctorId = _currentUserProvider.User?.ProfileId;
        if (doctorId is null)
            return Error.Validation("Doctor.ProfileRequired", "Doctor profile was not found.");

        var allowed = await _repository.HasApprovedWithPatientAsync(
            doctorId.Value,
            request.PatientId,
            cancellationToken);

        if (!allowed)
            return Error.Forbidden("Appointment.Forbidden", "You are not allowed to perform this action.");

        var (page, pageSize) = PageRequest.Normalize(request.Page, request.PageSize);

        var rows = await _repository.GetByPatientAsync(request.PatientId, page, pageSize, cancellationToken);

        var items = await _detailsResolver.ResolveAsync(rows.Items, includePatients: false, cancellationToken);

        return new PagedResult<AppointmentListItemDto>(items, rows.Page, rows.PageSize, rows.TotalCount);
    }
}
