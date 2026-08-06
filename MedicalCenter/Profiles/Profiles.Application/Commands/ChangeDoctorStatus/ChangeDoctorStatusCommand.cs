using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Domain.Constants;
using Profiles.Domain.Enums;

namespace Profiles.Application.Commands.ChangeDoctorStatus;

public record ChangeDoctorStatusCommand(Guid Id, DoctorStatus Status)
    : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ChangeDoctorStatus;
}
