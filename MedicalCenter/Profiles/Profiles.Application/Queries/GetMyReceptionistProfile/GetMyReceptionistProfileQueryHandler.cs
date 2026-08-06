using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetMyReceptionistProfile;

public sealed class GetMyReceptionistProfileQueryHandler
    : IRequestHandler<GetMyReceptionistProfileQuery, ErrorOr<ReceptionistDto>>
{
    private readonly IReceptionistQueryRepository _repository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetMyReceptionistProfileQueryHandler(
        IReceptionistQueryRepository repository,
        ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<ReceptionistDto>> Handle(
        GetMyReceptionistProfileQuery request,
        CancellationToken cancellationToken)
    {
        var accountId = _currentUserProvider.User?.Id;
        if (accountId is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var receptionist = await _repository.GetByAccountIdAsync(accountId.Value, cancellationToken);
        if (receptionist is null)
            return Error.NotFound("Receptionist.NotFound", "Receptionist profile was not found.");

        return receptionist;
    }
}
