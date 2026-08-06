using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Domain.Constants;

namespace Profiles.Application.Commands.DeleteReceptionist;

public record DeleteReceptionistCommand(Guid Id) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.DeleteReceptionist;
}
