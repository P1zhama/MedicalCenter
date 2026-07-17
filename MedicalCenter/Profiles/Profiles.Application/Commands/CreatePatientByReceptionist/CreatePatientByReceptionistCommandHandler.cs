using MediatR;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Commands.CreatePatientByReceptionist;

public class CreatePatientByReceptionistCommandHandler : IRequestHandler<CreatePatientByReceptionistCommand, Guid>
{
    private readonly IPatientRepository _repository;

    public CreatePatientByReceptionistCommandHandler(IPatientRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreatePatientByReceptionistCommand request, CancellationToken cancellationToken)
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            AccountId = null,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            DateOfBirth = request.DateOfBirth,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(patient, cancellationToken);

        return patient.Id;
    }
}
