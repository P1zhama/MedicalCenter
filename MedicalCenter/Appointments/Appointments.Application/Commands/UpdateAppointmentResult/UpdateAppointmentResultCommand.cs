using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.UpdateAppointmentResult;

public record UpdateAppointmentResultCommand(
    Guid AppointmentId,
    string Complaints,
    string Conclusion,
    string Recommendations,
    string? Diagnosis
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ManageResults;
}
