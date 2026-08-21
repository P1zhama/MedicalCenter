using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Queries.GetPatientsSummary;

public record GetPatientsSummaryQuery(IReadOnlyCollection<Guid> Ids)
    : IRequest<ErrorOr<IReadOnlyList<PatientSummaryDto>>>;
