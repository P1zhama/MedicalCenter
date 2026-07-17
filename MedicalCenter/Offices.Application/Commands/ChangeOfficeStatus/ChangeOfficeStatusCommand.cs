using MediatR;
using Offices.Domain.Enums;

namespace Offices.Application.Commands.ChangeOfficeStatus;

public record ChangeOfficeStatusCommand(
    Guid Id,
    OfficeStatus Status
) : IRequest<bool>;
