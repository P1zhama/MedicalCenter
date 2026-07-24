using ErrorOr;
using MediatR;

namespace Profiles.Application.Commands.CreateReceptionist;

public record CreateReceptionistCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    string Email,
    Guid OfficeId,
    string? PhotoUrl,
    string CreatedBy
) : IRequest<ErrorOr<Guid>>;
