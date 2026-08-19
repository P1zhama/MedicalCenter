using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.RescheduleAppointment;

public record RescheduleAppointmentCommand(
    Guid Id,
    Guid DoctorId,
    DateOnly Date,
    TimeOnly StartTime
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.RescheduleAppointment;
}

public record RescheduleMyAppointmentCommand(
    Guid Id,
    Guid DoctorId,
    DateOnly Date,
    TimeOnly StartTime
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.RescheduleOwnAppointment;
}
