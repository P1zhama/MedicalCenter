using MediatR;
using Offices.Application.Common.Interfaces;
using Offices.Domain;

namespace Offices.Application.Commands.CreateOffice;

public class CreateOfficeCommandHandler : IRequestHandler<CreateOfficeCommand, Guid>
{
    private readonly IOfficeRepository _repository;

    public CreateOfficeCommandHandler(IOfficeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateOfficeCommand request, CancellationToken cancellationToken)
    {
        var office = new Office
        {
            Id = Guid.NewGuid(),
            City = request.City,
            Street = request.Street,
            HouseNumber = request.HouseNumber,
            OfficeNumber = request.OfficeNumber,
            RegistryPhoneNumber = request.RegistryPhoneNumber,
            PhotoUrl = request.PhotoUrl,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(office, cancellationToken);

        return office.Id;
    }
}
