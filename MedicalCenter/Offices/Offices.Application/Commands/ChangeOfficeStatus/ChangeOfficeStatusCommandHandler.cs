using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Offices.Application.Common.Interfaces;

namespace Offices.Application.Commands.ChangeOfficeStatus;

public sealed class ChangeOfficeStatusCommandHandler : IRequestHandler<ChangeOfficeStatusCommand, ErrorOr<Success>>
{
    private readonly IOfficeRepository _officeRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public ChangeOfficeStatusCommandHandler(
        IOfficeRepository officeRepository,
        IEventPublisher eventPublisher,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _officeRepository = officeRepository;
        _eventPublisher = eventPublisher;
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

        var updated = await _officeRepository.UpdateAsync(office, expectedVersion, cancellationToken);
        if (!updated)
            return Error.Conflict("Office.ConcurrencyConflict", "Office was modified by another operation. Please retry.");

        if (wasActive && !office.IsActive)
            await _eventPublisher.PublishAsync(new OfficeDeactivatedEvent(office.Id, now.UtcDateTime), cancellationToken);

        return Result.Success;
    }
}
