using MediatR;

namespace Authorization.Application.Commands.CreateWorkerAccount;

public record CreateWorkerAccountCommand(
    string Email,
    string RoleName,
    string CreatedBy
) : IRequest<string>;