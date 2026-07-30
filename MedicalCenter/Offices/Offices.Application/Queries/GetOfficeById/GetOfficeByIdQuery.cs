using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Offices.Application.Common.Dtos;
using Offices.Domain.Constants;

namespace Offices.Application.Queries.GetOfficeById;

public record GetOfficeByIdQuery(Guid Id) : IRequest<ErrorOr<OfficeDto>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewOffices;
}
