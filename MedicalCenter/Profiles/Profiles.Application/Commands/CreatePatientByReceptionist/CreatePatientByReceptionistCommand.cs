using MediatR;
using System;

namespace Profiles.Application.Commands.CreatePatientByReceptionist;

public record CreatePatientByReceptionistCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth
) : IRequest<Guid>;
