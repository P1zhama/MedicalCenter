
using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Profiles.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Commands.LinkExistingPatient;

public class LinkExistingPatientCommandHandler : IRequestHandler<LinkExistingPatientCommand, bool>
{
    private readonly IPatientRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public LinkExistingPatientCommandHandler(IPatientRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(LinkExistingPatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _repository.GetByIdAsync(request.PatientId, cancellationToken);

        if (patient == null || patient.AccountId.HasValue)
            return false;

        patient.AccountId = request.AccountId;
        patient.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(patient, cancellationToken);

        await _publishEndpoint.Publish(new ProfileLinkedToAccountEvent(
            request.AccountId,
            patient.Id,
            DateTime.UtcNow
        ), cancellationToken);

        return true;
    }
}