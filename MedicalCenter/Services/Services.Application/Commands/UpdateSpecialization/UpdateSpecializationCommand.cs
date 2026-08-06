using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Services.Domain.Constants;
using Services.Domain.Enums;

namespace Services.Application.Commands.UpdateSpecialization;

public record UpdateSpecializationCommand(
    Guid Id,
    string Name,
    ActivityStatus Status
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.EditSpecialization;
}
