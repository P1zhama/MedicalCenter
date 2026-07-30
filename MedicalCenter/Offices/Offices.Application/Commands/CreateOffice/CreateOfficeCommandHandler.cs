using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Offices.Application.Common.Interfaces;
using Offices.Domain.Models;
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
        var address = Address.Create(request.City, request.Street, request.HouseNumber, request.OfficeNumber);

        var now = _timeProvider.GetUtcNow();
        var createdBy = _currentUserProvider.User?.Id ?? Guid.Empty;

        var office = Office.Create(
            _guidProvider.NewGuid(),
            address,
            request.RegistryPhoneNumber,
            request.PhotoUrl,
            request.Status,
            createdBy,
            now);

        await _officeRepository.AddAsync(office, cancellationToken);

        return office.Id;
    }
}
