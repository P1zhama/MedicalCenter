using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;

namespace Services.Application.Queries.GetServicesSummary;

public sealed class GetServicesSummaryQueryHandler
    : IRequestHandler<GetServicesSummaryQuery, ErrorOr<IReadOnlyList<ServiceSummaryDto>>>
{
    private readonly IServiceQueryRepository _repository;

    public GetServicesSummaryQueryHandler(IServiceQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<ServiceSummaryDto>>> Handle(
        GetServicesSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var services = await _repository.GetSummariesAsync(request.Ids, cancellationToken);

        return ErrorOrFactory.From(services);
    }
}
