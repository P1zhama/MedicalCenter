using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Commands.ForceCreatePatient;

public class ForceCreatePatientCommandHandler : IRequestHandler<ForceCreatePatientCommand, Guid>
{
    private readonly IPatientRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public ForceCreatePatientCommandHandler(
        IPatientRepository repository,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(ForceCreatePatientCommand request, CancellationToken cancellationToken)
    {
        var newPatient = new Patient
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            PhotoUrl = request.PhotoUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(newPatient, cancellationToken);

        await _publishEndpoint.Publish(new ProfileLinkedToAccountEvent(
            request.AccountId,
            newPatient.Id,
            DateTime.UtcNow
        ), cancellationToken);

        return newPatient.Id;
    }
}
