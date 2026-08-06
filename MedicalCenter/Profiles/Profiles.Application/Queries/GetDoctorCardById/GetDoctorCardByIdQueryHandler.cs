using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetDoctorCardById;

public sealed class GetDoctorCardByIdQueryHandler : IRequestHandler<GetDoctorCardByIdQuery, ErrorOr<DoctorCardDto>>
{
    private readonly IDoctorQueryRepository _repository;
    private readonly TimeProvider _timeProvider;

    public GetDoctorCardByIdQueryHandler(IDoctorQueryRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<DoctorCardDto>> Handle(
        GetDoctorCardByIdQuery request,
        CancellationToken cancellationToken)
    {
        var doctor = await _repository.GetActiveCardByIdAsync(
            request.Id,
            _timeProvider.GetUtcNow().Year,
            cancellationToken);

        if (doctor is null)
            return Error.NotFound("Doctor.NotFound", "Doctor was not found.");

        return doctor;
    }
}
