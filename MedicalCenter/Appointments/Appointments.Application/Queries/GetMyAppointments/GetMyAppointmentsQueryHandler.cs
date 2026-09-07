using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using Common.Abstractions.Security;
using Common.Abstractions.Paging;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetMyAppointments;

public sealed class GetMyAppointmentsQueryHandler
    : IRequestHandler<GetMyAppointmentsQuery, ErrorOr<PagedResult<AppointmentListItemDto>>>
{
    private readonly IAppointmentQueryRepository _repository;
    private readonly AppointmentDetailsResolver _detailsResolver;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetMyAppointmentsQueryHandler(
        IAppointmentQueryRepository repository,
        AppointmentDetailsResolver detailsResolver,
        ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _detailsResolver = detailsResolver;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<PagedResult<AppointmentListItemDto>>> Handle(
        GetMyAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = _currentUserProvider.User?.ProfileId;
        if (patientId is null)
            return Error.Validation("Patient.ProfileRequired", "Please, create your profile first.");

        var (page, pageSize) = PageRequest.Normalize(request.Page, request.PageSize);

        var rows = await _repository.GetByPatientAsync(patientId.Value, page, pageSize, cancellationToken);

        var items = await _detailsResolver.ResolveAsync(rows.Items, includePatients: false, cancellationToken);

        return new PagedResult<AppointmentListItemDto>(items, rows.Page, rows.PageSize, rows.TotalCount);
    }
}
