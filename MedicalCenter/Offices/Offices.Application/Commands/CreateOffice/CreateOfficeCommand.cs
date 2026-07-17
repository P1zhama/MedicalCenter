using MediatR;
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
) : IRequest<Guid>;
