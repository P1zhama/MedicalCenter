using MediatR;
using Profiles.Domain.Enums;
using System;

namespace Profiles.Application.Commands.CreateDoctor;

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
