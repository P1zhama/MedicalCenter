using Appointments.Application.Common.Dtos;
using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetAppointments;

public record GetAppointmentsQuery(
    DateOnly Date,
    Guid? DoctorId,
    Guid? ServiceId,
    Guid? OfficeId,
    string? Status
) : IRequest<ErrorOr<IReadOnlyList<AppointmentListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewAppointments;
}
