using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Services.Domain.Constants;
using Services.Domain.Enums;

namespace Services.Application.Commands.ChangeSpecializationStatus;

public record ChangeSpecializationStatusCommand(
    Guid Id,
    ActivityStatus Status
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ChangeSpecializationStatus;
}
