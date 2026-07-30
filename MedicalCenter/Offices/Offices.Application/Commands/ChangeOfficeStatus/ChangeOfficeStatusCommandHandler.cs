using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Offices.Application.Common.Events;
using Offices.Application.Common.Interfaces;

namespace Offices.Application.Commands.ChangeOfficeStatus;

public sealed class ChangeOfficeStatusCommandHandler : IRequestHandler<ChangeOfficeStatusCommand, ErrorOr<Success>>
{
    private readonly IOfficeCommandRepository _officeRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public ChangeOfficeStatusCommandHandler(
        IOfficeCommandRepository officeRepository,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _officeRepository = officeRepository;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(ChangeOfficeStatusCommand request, CancellationToken cancellationToken)
    {
        var office = await _officeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (office is null)
            return Error.NotFound("Office.NotFound", "Office was not found.");

        var now = _timeProvider.GetUtcNow();
        var updatedBy = _currentUserProvider.User?.Id ?? Guid.Empty;
        var wasActive = office.IsActive;
        var expectedVersion = office.Version;

        office.ChangeStatus(request.Status, updatedBy, now);

        var integrationEvents = OfficeIntegrationEvents.ForStatusChange(wasActive, office, now);

        var updated = await _officeRepository.UpdateAsync(office, expectedVersion, integrationEvents, cancellationToken);
        if (!updated)
            return Error.Conflict("Office.ConcurrencyConflict", "Office was modified by another operation. Please retry.");

        return Result.Success;
    }
}
