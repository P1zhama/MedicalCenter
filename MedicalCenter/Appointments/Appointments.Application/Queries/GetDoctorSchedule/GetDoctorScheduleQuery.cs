using Appointments.Application.Common.Dtos;
using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetDoctorSchedule;

public record GetDoctorScheduleQuery(DateOnly Date)
    : IRequest<ErrorOr<IReadOnlyList<AppointmentListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewDoctorSchedule;
}
