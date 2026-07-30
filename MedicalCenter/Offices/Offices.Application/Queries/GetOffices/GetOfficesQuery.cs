using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Offices.Application.Common.Dtos;
using Offices.Domain.Constants;

namespace Offices.Application.Queries.GetOffices;

public record GetOfficesQuery() : IRequest<ErrorOr<IReadOnlyList<OfficeListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewOffices;
}
