using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;

namespace Services.Application.Queries.GetServicesSummary;

public record GetServicesSummaryQuery(IReadOnlyCollection<Guid> Ids)
    : IRequest<ErrorOr<IReadOnlyList<ServiceSummaryDto>>>;
