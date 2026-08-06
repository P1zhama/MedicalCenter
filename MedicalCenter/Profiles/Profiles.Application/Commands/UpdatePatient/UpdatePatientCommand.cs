using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Domain.Constants;

namespace Profiles.Application.Commands.UpdatePatient;

public record UpdatePatientCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhoneNumber,
    DateOnly DateOfBirth,
    string? PhotoUrl
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.EditPatient;
}
