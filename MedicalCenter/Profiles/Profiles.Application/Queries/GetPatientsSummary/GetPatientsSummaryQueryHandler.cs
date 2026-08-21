using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Behaviors;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetPatientsSummary;

public sealed class GetPatientsSummaryQueryHandler
    : IRequestHandler<GetPatientsSummaryQuery, ErrorOr<IReadOnlyList<PatientSummaryDto>>>
{
    private readonly IPatientQueryRepository _repository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetPatientsSummaryQueryHandler(
        IPatientQueryRepository repository,
        ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<IReadOnlyList<PatientSummaryDto>>> Handle(
        GetPatientsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.User;

        if (user is null || !user.IsAuthenticated)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var isSelfLookup = user.HasProfile
            && request.Ids.Count == 1
            && request.Ids.Contains(user.ProfileId!.Value);

        if (!isSelfLookup && !PermissionCheck.Has(user, Permissions.ViewPatients))
            return Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");

        var patients = await _repository.GetSummariesAsync(request.Ids, cancellationToken);

        return ErrorOrFactory.From(patients);
    }
}
