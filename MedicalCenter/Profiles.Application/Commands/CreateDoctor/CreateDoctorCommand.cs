using MediatR;
using Profiles.Domain.Enums;
using System;

namespace Profiles.Application.Commands.CreateDoctor;

// US-9: регистратор создаёт профиль доктора. CreatedBy — email того, кто создаёт;
// приходит не от клиента, а из проверенного JWT на Gateway (см. Gateway ProfilesController)
public record CreateDoctorCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth,
    string Email,
    Guid SpecializationId,
    Guid OfficeId,
    int CareerStartYear,
    DoctorStatus Status,
    string? PhotoUrl,
    string CreatedBy
) : IRequest<Guid>;
