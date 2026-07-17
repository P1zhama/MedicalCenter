using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Offices.Application.Common.Interfaces;
using Offices.Domain.Enums;

namespace Offices.Application.Commands.UpdateOffice;

public class UpdateOfficeCommandHandler : IRequestHandler<UpdateOfficeCommand, bool>
{
    private readonly IOfficeRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateOfficeCommandHandler(IOfficeRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(UpdateOfficeCommand request, CancellationToken cancellationToken)
    {
        var office = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Office {request.Id} was not found.");

        var wasActive = office.Status != OfficeStatus.Inactive;

        office.City = request.City;
        office.Street = request.Street;
        office.HouseNumber = request.HouseNumber;
        office.OfficeNumber = request.OfficeNumber;
        office.RegistryPhoneNumber = request.RegistryPhoneNumber;
        office.PhotoUrl = request.PhotoUrl;
        office.Status = request.Status;
        office.UpdatedAt = DateTime.UtcNow;


        await _repository.UpdateAsync(office, cancellationToken);


        if (wasActive && request.Status == OfficeStatus.Inactive)
        {
            await _publishEndpoint.Publish(new OfficeDeactivatedEvent(office.Id, DateTime.UtcNow), cancellationToken);
        }

        return true;
    }
}
