using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Domain.Constants;

namespace Profiles.Application.Queries.GetDoctorById;

public record GetDoctorByIdQuery(Guid Id) : IRequest<ErrorOr<DoctorDto>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewDoctors;
}
