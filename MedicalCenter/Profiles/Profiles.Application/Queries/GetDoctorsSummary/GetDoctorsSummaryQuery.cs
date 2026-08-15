using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Queries.GetDoctorsSummary;

public record GetDoctorsSummaryQuery(IReadOnlyCollection<Guid> Ids)
    : IRequest<ErrorOr<IReadOnlyList<DoctorSummaryDto>>>;
