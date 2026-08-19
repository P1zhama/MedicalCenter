using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.CreateAppointmentResult;

public record CreateAppointmentResultCommand(
    Guid AppointmentId,
    string Complaints,
    string Conclusion,
    string Recommendations,
    string? Diagnosis
) : IRequest<ErrorOr<Guid>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ManageResults;
}
