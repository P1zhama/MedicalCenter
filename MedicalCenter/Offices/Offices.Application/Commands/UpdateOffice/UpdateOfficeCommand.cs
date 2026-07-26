using ErrorOr;
using MediatR;
using Offices.Application.Common.Security;
using Offices.Domain.Constants;
using Offices.Domain.Enums;

namespace Offices.Application.Commands.UpdateOffice;

public record UpdateOfficeCommand(
    Guid Id,
    string City,
    string Street,
    string HouseNumber,
    string? OfficeNumber,
    string RegistryPhoneNumber,
    string? PhotoUrl,
    OfficeStatus Status
) : IRequest<ErrorOr<Success>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.EditOffice;
}
