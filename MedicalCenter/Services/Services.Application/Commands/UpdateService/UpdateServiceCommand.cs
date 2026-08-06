using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Services.Domain.Constants;
using Services.Domain.Enums;

namespace Services.Application.Commands.UpdateService;

public record UpdateServiceCommand(
    Guid Id,
    string Name,
    decimal Price,
    Guid CategoryId,
    ActivityStatus Status
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.EditService;
}
