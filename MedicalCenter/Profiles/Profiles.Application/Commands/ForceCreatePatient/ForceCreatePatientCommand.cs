using ErrorOr;
using MediatR;

namespace Profiles.Application.Commands.ForceCreatePatient;

public record ForceCreatePatientCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    string PhoneNumber,
    DateOnly DateOfBirth,
    string? PhotoUrl
) : IRequest<ErrorOr<Guid>>;
