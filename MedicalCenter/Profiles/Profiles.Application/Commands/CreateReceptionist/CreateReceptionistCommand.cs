using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Domain.Constants;

namespace Profiles.Application.Commands.CreateReceptionist;

public record CreateReceptionistCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    string Email,
    Guid OfficeId,
    string? PhotoUrl
) : IRequest<ErrorOr<Guid>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.CreateReceptionist;
}
