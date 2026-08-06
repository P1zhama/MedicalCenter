using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetDoctorCards;

public sealed class GetDoctorCardsQueryHandler
    : IRequestHandler<GetDoctorCardsQuery, ErrorOr<IReadOnlyList<DoctorCardDto>>>
{
    private readonly IDoctorQueryRepository _repository;
    private readonly TimeProvider _timeProvider;

    public GetDoctorCardsQueryHandler(IDoctorQueryRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<IReadOnlyList<DoctorCardDto>>> Handle(
        GetDoctorCardsQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new DoctorFilter(request.Search, request.SpecializationId, request.OfficeId);

        var doctors = await _repository.GetActiveCardsAsync(
            filter,
            _timeProvider.GetUtcNow().Year,
            cancellationToken);

        return ErrorOrFactory.From(doctors);
    }
}
