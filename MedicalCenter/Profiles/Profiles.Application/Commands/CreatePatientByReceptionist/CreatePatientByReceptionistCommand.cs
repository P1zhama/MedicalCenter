using ErrorOr;
using MediatR;

namespace Profiles.Application.Commands.CreatePatientByReceptionist;

public record CreatePatientByReceptionistCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth
) : IRequest<ErrorOr<Guid>>;
