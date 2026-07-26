using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using Offices.Application.Common.Interfaces;
using Offices.Domain;
using Offices.Domain.ValueObjects;

namespace Offices.Application.Commands.CreateOffice;

public sealed class CreateOfficeCommandHandler : IRequestHandler<CreateOfficeCommand, ErrorOr<Guid>>
{
    private readonly IOfficeRepository _officeRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;

    public CreateOfficeCommandHandler(
        IOfficeRepository officeRepository,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider,
        IGuidProvider guidProvider)
    {
        _officeRepository = officeRepository;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateOfficeCommand request, CancellationToken cancellationToken)
    {
        var addressResult = Address.Create(request.City, request.Street, request.HouseNumber, request.OfficeNumber);
        if (addressResult.IsError)
            return addressResult.Errors;

        var now = _timeProvider.GetUtcNow();
        var createdBy = _currentUserProvider.User?.Id ?? Guid.Empty;

        var officeResult = Office.Create(
            _guidProvider.NewGuid(),
            addressResult.Value,
            request.RegistryPhoneNumber,
            request.PhotoUrl,
            request.Status,
            createdBy,
            now);

        if (officeResult.IsError)
            return officeResult.Errors;

        await _officeRepository.AddAsync(officeResult.Value, cancellationToken);

        return officeResult.Value.Id;
    }
}
