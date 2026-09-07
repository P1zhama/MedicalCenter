using Common.Abstractions.Security;
using Common.Abstractions.Paging;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetDoctors;

public record GetDoctorsQuery(string? Search, Guid? SpecializationId, Guid? OfficeId, int Page, int PageSize)
    : IRequest<ErrorOr<PagedResult<DoctorListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewDoctors;
}
