using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetDoctors;

public record GetDoctorsQuery(string? Search, Guid? SpecializationId, Guid? OfficeId)
    : IRequest<ErrorOr<IReadOnlyList<DoctorListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewDoctors;
}
