using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Services.Domain.Constants;
using Services.Domain.Enums;

namespace Services.Application.Commands.CreateSpecialization;

public record CreateSpecializationServiceItem(
    string Name,
    decimal Price,
    Guid CategoryId,
    ActivityStatus Status);

public record CreateSpecializationCommand(
    string Name,
    ActivityStatus Status,
    IReadOnlyList<CreateSpecializationServiceItem> Services
) : IRequest<ErrorOr<Guid>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.CreateSpecialization;
}
