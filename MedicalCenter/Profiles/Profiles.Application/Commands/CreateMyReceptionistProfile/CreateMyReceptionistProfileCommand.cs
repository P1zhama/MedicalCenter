using ErrorOr;
using MediatR;

namespace Profiles.Application.Commands.CreateMyReceptionistProfile;

public record CreateMyReceptionistProfileCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    Guid OfficeId,
    string? PhotoUrl
) : IRequest<ErrorOr<Guid>>;
