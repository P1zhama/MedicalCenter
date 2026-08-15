using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetPatientsSummary;

public record GetPatientsSummaryQuery(IReadOnlyCollection<Guid> Ids)
    : IRequest<ErrorOr<IReadOnlyList<PatientSummaryDto>>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewPatients;
}
