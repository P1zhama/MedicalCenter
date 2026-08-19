using Appointments.Application.Common.Dtos;
using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetAppointmentResult;

public record GetAppointmentResultQuery(Guid AppointmentId)
    : IRequest<ErrorOr<AppointmentResultDto>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewResults;
}
