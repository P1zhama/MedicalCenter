using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetPatients;

public record GetPatientsQuery(string? Search)
    : IRequest<ErrorOr<IReadOnlyList<PatientListItemDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewPatients;
}
