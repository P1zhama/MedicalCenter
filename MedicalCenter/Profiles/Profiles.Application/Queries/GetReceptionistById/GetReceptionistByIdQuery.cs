using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetReceptionistById;

public record GetReceptionistByIdQuery(Guid Id) : IRequest<ErrorOr<ReceptionistDto>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewReceptionists;
}
