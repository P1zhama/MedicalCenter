using MediatR;
using System;

namespace Profiles.Application.Commands.CreateReceptionist;

// US-53: регистратор создаёт профиль другого регистратора. CreatedBy — email того, кто создаёт;
// приходит не от клиента, а из проверенного JWT на Gateway (см. Gateway ProfilesController)
public record CreateReceptionistCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    string Email,
    Guid OfficeId,
    string? PhotoUrl,
    string CreatedBy
) : IRequest<Guid>;
