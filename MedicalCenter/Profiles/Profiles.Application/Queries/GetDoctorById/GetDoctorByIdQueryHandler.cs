using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetDoctorById;

public sealed class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, ErrorOr<DoctorDto>>
{
    private readonly IDoctorQueryRepository _repository;
    private readonly TimeProvider _timeProvider;

    public GetDoctorByIdQueryHandler(IDoctorQueryRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<DoctorDto>> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _repository.GetByIdAsync(
            request.Id,
            _timeProvider.GetUtcNow().Year,
            cancellationToken);

        if (doctor is null)
            return Error.NotFound("Doctor.NotFound", "Doctor was not found.");

        return doctor;
    }
}
