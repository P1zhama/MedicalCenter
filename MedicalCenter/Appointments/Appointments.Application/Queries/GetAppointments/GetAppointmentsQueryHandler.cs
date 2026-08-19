using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetAppointments;

public sealed class GetAppointmentsQueryHandler
    : IRequestHandler<GetAppointmentsQuery, ErrorOr<IReadOnlyList<AppointmentListItemDto>>>
{
    private readonly IAppointmentQueryRepository _repository;
    private readonly AppointmentDetailsResolver _detailsResolver;

    public GetAppointmentsQueryHandler(IAppointmentQueryRepository repository, AppointmentDetailsResolver detailsResolver)
    {
        _repository = repository;
        _detailsResolver = detailsResolver;
    }

    public async Task<ErrorOr<IReadOnlyList<AppointmentListItemDto>>> Handle(
        GetAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new AppointmentFilter(
            request.Date,
            request.DoctorId,
            request.ServiceId,
            request.OfficeId,
            request.Status);

        var rows = await _repository.SearchAsync(filter, cancellationToken);

        var items = await _detailsResolver.ResolveAsync(rows, includePatients: true, cancellationToken);

        var ordered = items
            .OrderBy(item => item.StartTime)
            .ThenBy(item => item.DoctorLastName, StringComparer.CurrentCulture)
            .ThenBy(item => item.DoctorFirstName, StringComparer.CurrentCulture)
            .ThenBy(item => item.ServiceName, StringComparer.CurrentCulture)
            .ToList();

        return ErrorOrFactory.From<IReadOnlyList<AppointmentListItemDto>>(ordered);
    }
}
