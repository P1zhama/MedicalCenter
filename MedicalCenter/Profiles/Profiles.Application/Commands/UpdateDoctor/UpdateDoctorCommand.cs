using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Profiles.Domain.Constants;
using Profiles.Domain.Enums;

namespace Profiles.Application.Commands.UpdateDoctor;

public record UpdateDoctorCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth,
    Guid SpecializationId,
    Guid OfficeId,
    int CareerStartYear,
    DoctorStatus Status,
    string? PhotoUrl
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.EditDoctor;
}
