using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetReceptionists;

public record GetReceptionistsQuery()
    : IRequest<ErrorOr<IReadOnlyList<ReceptionistListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewReceptionists;
}
