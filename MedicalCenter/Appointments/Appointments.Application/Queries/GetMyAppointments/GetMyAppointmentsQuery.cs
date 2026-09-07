using Appointments.Application.Common.Dtos;
using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using Common.Abstractions.Paging;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetMyAppointments;

public record GetMyAppointmentsQuery(int Page, int PageSize)
    : IRequest<ErrorOr<PagedResult<AppointmentListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewOwnAppointments;
}
