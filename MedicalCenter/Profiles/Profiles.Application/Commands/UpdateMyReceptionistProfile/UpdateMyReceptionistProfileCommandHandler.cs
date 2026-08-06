using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain.ValueObjects;

namespace Profiles.Application.Commands.UpdateMyReceptionistProfile;

public sealed class UpdateMyReceptionistProfileCommandHandler
    : IRequestHandler<UpdateMyReceptionistProfileCommand, ErrorOr<Success>>
{
    private readonly IReceptionistCommandRepository _receptionistRepository;
    private readonly IReceptionistQueryRepository _receptionistQueryRepository;
    private readonly IOfficeServiceClient _officeServiceClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public UpdateMyReceptionistProfileCommandHandler(
        IReceptionistCommandRepository receptionistCommandRepository,
        IReceptionistQueryRepository receptionistQueryRepository,
        IOfficeServiceClient officeServiceClient,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _receptionistRepository = receptionistCommandRepository;
        _receptionistQueryRepository = receptionistQueryRepository;
        _officeServiceClient = officeServiceClient;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateMyReceptionistProfileCommand request,
        CancellationToken cancellationToken)
    {
        var accountId = _currentUserProvider.User?.Id;
        if (accountId is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var own = await _receptionistQueryRepository.GetByAccountIdAsync(accountId.Value, cancellationToken);
        if (own is null)
            return Error.NotFound("Receptionist.NotFound", "Receptionist profile was not found.");

        var receptionist = await _receptionistRepository.GetByIdAsync(own.Id, cancellationToken);
        if (receptionist is null)
            return Error.NotFound("Receptionist.NotFound", "Receptionist profile was not found.");

        var nameResult = PersonName.Create(request.FirstName, request.LastName, request.MiddleName);
        if (nameResult.IsError)
            return nameResult.Errors;

        if (receptionist.OfficeId != request.OfficeId
            && !await _officeServiceClient.IsOfficeActiveAsync(request.OfficeId, cancellationToken))
        {
            return Error.Validation("Receptionist.OfficeId", "Please, choose the office");
        }

        var now = _timeProvider.GetUtcNow();
        var expectedVersion = receptionist.Version;

        var updateResult = receptionist.Update(
            nameResult.Value,
            request.OfficeId,
            receptionist.Status,
            request.PhotoUrl,
            accountId.Value,
            now);

        if (updateResult.IsError)
            return updateResult.Errors;

        _receptionistRepository.Update(receptionist, expectedVersion);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Receptionist.ConcurrencyConflict", "Receptionist was modified by another operation. Please retry.");

        return Result.Success;
    }
}
