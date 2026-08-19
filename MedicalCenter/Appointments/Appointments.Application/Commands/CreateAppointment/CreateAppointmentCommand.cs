using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.CreateAppointment;

public record CreateAppointmentCommand(
    Guid ServiceId,
    Guid DoctorId,
    Guid OfficeId,
    DateOnly Date,
    TimeOnly StartTime
) : IRequest<ErrorOr<Guid>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.CreateOwnAppointment;
}
