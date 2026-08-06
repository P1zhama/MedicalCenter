using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;
using Services.Domain.Constants;

namespace Services.Application.Queries.GetServiceById;

public record GetServiceByIdQuery(Guid Id) : IRequest<ErrorOr<ServiceDto>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewServices;
}
