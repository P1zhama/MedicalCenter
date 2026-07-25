using ErrorOr;
using MediatR;
using Profiles.Application.Common.Security;
using Profiles.Domain.Constants;
using Profiles.Domain.Enums;

namespace Profiles.Application.Commands.CreateDoctor;

public record CreateDoctorCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth,
    string Email,
    Guid SpecializationId,
    Guid OfficeId,
    int CareerStartYear,
    DoctorStatus Status,
    string? PhotoUrl,
    string CreatedBy
) : IRequest<ErrorOr<Guid>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.CreateDoctor;
}
