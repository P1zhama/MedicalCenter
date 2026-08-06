using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Domain.Constants;

namespace Profiles.Application.Commands.CreatePatientByReceptionist;

public record CreatePatientByReceptionistCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth
) : IRequest<ErrorOr<Guid>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.CreatePatient;
}
