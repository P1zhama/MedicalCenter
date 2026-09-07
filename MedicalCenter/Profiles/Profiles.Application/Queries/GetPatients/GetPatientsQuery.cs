using Common.Abstractions.Paging;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetPatients;

public record GetPatientsQuery(string? Search, int Page, int PageSize)
    : IRequest<ErrorOr<PagedResult<PatientListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewPatients;
}
