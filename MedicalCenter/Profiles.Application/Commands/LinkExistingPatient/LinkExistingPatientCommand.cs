using MediatR;
using System;

namespace Profiles.Application.Commands.LinkExistingPatient;

public record LinkExistingPatientCommand(
    Guid AccountId,
    Guid PatientId
) : IRequest<bool>; // Возвращаем true, если успешно привязали