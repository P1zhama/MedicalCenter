using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Services.Domain.Constants;
using Services.Domain.Enums;

namespace Services.Application.Commands.CreateService;

public record CreateServiceCommand(
    string Name,
    decimal Price,
    Guid SpecializationId,
    Guid CategoryId,
    ActivityStatus Status
) : IRequest<ErrorOr<Guid>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.CreateService;
}
