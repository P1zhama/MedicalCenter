using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Offices.Application.Common.Interfaces;
using Offices.Domain.ValueObjects;

namespace Offices.Application.Commands.UpdateOffice;

public sealed class UpdateOfficeCommandHandler : IRequestHandler<UpdateOfficeCommand, ErrorOr<Success>>
{
    private readonly IOfficeRepository _officeRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public UpdateOfficeCommandHandler(
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

    public async Task<ErrorOr<Success>> Handle(UpdateOfficeCommand request, CancellationToken cancellationToken)
    {
        var office = await _officeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (office is null)
            return Error.NotFound("Office.NotFound", "Office was not found.");

        var addressResult = Address.Create(request.City, request.Street, request.HouseNumber, request.OfficeNumber);
        if (addressResult.IsError)
            return addressResult.Errors;

        var now = _timeProvider.GetUtcNow();
        var updatedBy = _currentUserProvider.User?.Id ?? Guid.Empty;
        var wasActive = office.IsActive;
        var expectedVersion = office.Version;

        var updateResult = office.Update(
            addressResult.Value,
            request.RegistryPhoneNumber,
            request.PhotoUrl,
            request.Status,
            updatedBy,
            now);

        if (updateResult.IsError)
            return updateResult.Errors;

        var updated = await _officeRepository.UpdateAsync(office, expectedVersion, cancellationToken);
        if (!updated)
            return Error.Conflict("Office.ConcurrencyConflict", "Office was modified by another operation. Please retry.");

        if (wasActive && !office.IsActive)
            await _eventPublisher.PublishAsync(new OfficeDeactivatedEvent(office.Id, now.UtcDateTime), cancellationToken);

        return Result.Success;
    }
}
