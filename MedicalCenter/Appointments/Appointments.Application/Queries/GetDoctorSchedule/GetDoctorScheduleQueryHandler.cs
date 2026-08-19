using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetDoctorSchedule;

public sealed class GetDoctorScheduleQueryHandler
    : IRequestHandler<GetDoctorScheduleQuery, ErrorOr<IReadOnlyList<AppointmentListItemDto>>>
{
    private readonly IAppointmentQueryRepository _repository;
    private readonly AppointmentDetailsResolver _detailsResolver;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetDoctorScheduleQueryHandler(
        IAppointmentQueryRepository repository,
        AppointmentDetailsResolver detailsResolver,
        ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _detailsResolver = detailsResolver;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<IReadOnlyList<AppointmentListItemDto>>> Handle(
        GetDoctorScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var doctorId = _currentUserProvider.User?.ProfileId;
        if (doctorId is null)
            return Error.Validation("Doctor.ProfileRequired", "Doctor profile was not found.");

        var rows = await _repository.GetByDoctorAndDateAsync(doctorId.Value, request.Date, cancellationToken);

        var items = await _detailsResolver.ResolveAsync(rows, includePatients: true, cancellationToken);

        return ErrorOrFactory.From(items);
    }
}
