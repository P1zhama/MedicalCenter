using Appointments.Application.Common.Dtos;
using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetPatientAppointments;

public record GetPatientAppointmentsQuery(Guid PatientId)
    : IRequest<ErrorOr<IReadOnlyList<AppointmentListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewPatientHistory;
}
