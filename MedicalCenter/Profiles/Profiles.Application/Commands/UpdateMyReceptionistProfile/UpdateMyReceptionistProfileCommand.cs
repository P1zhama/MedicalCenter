using ErrorOr;
using MediatR;

namespace Profiles.Application.Commands.UpdateMyReceptionistProfile;

public record UpdateMyReceptionistProfileCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    Guid OfficeId,
    string? PhotoUrl
) : IRequest<ErrorOr<Success>>;
