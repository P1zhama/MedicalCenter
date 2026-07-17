using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Offices.Application.Common.Interfaces;
using Offices.Domain.Enums;

namespace Offices.Application.Commands.ChangeOfficeStatus;

public class ChangeOfficeStatusCommandHandler : IRequestHandler<ChangeOfficeStatusCommand, bool>
{
    private readonly IOfficeRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public ChangeOfficeStatusCommandHandler(IOfficeRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(ChangeOfficeStatusCommand request, CancellationToken cancellationToken)
    {
        var office = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Office {request.Id} was not found.");

        var wasActive = office.Status != OfficeStatus.Inactive;

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
