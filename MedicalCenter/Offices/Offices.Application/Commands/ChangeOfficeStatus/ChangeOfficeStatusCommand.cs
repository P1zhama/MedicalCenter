using ErrorOr;
using MediatR;
using Offices.Application.Common.Security;
using Offices.Domain.Constants;
using Offices.Domain.Enums;

namespace Offices.Application.Commands.ChangeOfficeStatus;

public record ChangeOfficeStatusCommand(
    Guid Id,
    OfficeStatus Status
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ChangeOfficeStatus;
}
