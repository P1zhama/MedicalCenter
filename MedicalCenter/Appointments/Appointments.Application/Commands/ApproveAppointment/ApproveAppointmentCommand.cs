using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.ApproveAppointment;

public record ApproveAppointmentCommand(Guid Id) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ApproveAppointment;
}
