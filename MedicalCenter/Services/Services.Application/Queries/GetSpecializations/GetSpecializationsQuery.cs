using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;
using Services.Domain.Constants;

namespace Services.Application.Queries.GetSpecializations;

public record GetSpecializationsQuery() : IRequest<ErrorOr<IReadOnlyList<SpecializationListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewSpecializations;
}
