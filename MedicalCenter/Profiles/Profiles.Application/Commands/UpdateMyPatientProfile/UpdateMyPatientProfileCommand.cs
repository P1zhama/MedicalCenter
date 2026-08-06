using ErrorOr;
using MediatR;

namespace Profiles.Application.Commands.UpdateMyPatientProfile;

public record UpdateMyPatientProfileCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhoneNumber,
    DateOnly DateOfBirth,
    string? PhotoUrl
) : IRequest<ErrorOr<Success>>;
