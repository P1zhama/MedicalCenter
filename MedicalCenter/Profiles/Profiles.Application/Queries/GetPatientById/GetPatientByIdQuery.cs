using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetPatientById;

public record GetPatientByIdQuery(Guid Id) : IRequest<ErrorOr<PatientDto>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewPatients;
}
