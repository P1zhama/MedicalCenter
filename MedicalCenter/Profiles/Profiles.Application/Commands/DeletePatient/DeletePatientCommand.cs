using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Domain.Constants;

namespace Profiles.Application.Commands.DeletePatient;

public record DeletePatientCommand(Guid Id) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.DeletePatient;
}
