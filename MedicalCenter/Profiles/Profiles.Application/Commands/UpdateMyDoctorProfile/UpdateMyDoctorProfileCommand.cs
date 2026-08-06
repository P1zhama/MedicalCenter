using ErrorOr;
using MediatR;

namespace Profiles.Application.Commands.UpdateMyDoctorProfile;

public record UpdateMyDoctorProfileCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth,
    Guid SpecializationId,
    Guid OfficeId,
    int CareerStartYear,
    string? PhotoUrl
) : IRequest<ErrorOr<Success>>;
