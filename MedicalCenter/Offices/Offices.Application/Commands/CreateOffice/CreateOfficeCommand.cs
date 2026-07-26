using ErrorOr;
using MediatR;
using Offices.Application.Common.Security;
using Offices.Domain.Constants;
using Offices.Domain.Enums;

namespace Offices.Application.Commands.CreateOffice;

public record CreateOfficeCommand(
    string City,
    string Street,
    string HouseNumber,
    string? OfficeNumber,
    string RegistryPhoneNumber,
    string? PhotoUrl,
    OfficeStatus Status
) : IRequest<ErrorOr<Guid>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.CreateOffice;
}
