using ErrorOr;
using MediatR;
using Profiles.Application.Common.Security;
using Profiles.Domain.Constants;

namespace Profiles.Application.Commands.CreateReceptionist;

public record CreateReceptionistCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    string Email,
    Guid OfficeId,
    string? PhotoUrl,
    string CreatedBy
) : IRequest<ErrorOr<Guid>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.CreateReceptionist;
}
