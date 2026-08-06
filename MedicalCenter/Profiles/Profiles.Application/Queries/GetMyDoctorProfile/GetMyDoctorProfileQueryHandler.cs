using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetMyDoctorProfile;

public sealed class GetMyDoctorProfileQueryHandler : IRequestHandler<GetMyDoctorProfileQuery, ErrorOr<DoctorDto>>
{
    private readonly IDoctorQueryRepository _repository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public GetMyDoctorProfileQueryHandler(
        IDoctorQueryRepository repository,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<DoctorDto>> Handle(GetMyDoctorProfileQuery request, CancellationToken cancellationToken)
    {
        var accountId = _currentUserProvider.User?.Id;
        if (accountId is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var doctor = await _repository.GetByAccountIdAsync(
            accountId.Value,
            _timeProvider.GetUtcNow().Year,
            cancellationToken);

        if (doctor is null)
            return Error.NotFound("Doctor.NotFound", "Doctor profile was not found.");

        return doctor;
    }
}
