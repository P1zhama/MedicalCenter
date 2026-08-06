using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Domain.Constants;
using Profiles.Domain.Enums;

namespace Profiles.Application.Commands.UpdateReceptionist;

public record UpdateReceptionistCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    Guid OfficeId,
    ReceptionistStatus Status,
    string? PhotoUrl
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.EditReceptionist;
}
