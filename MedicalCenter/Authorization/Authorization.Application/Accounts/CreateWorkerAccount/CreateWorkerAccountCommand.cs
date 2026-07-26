using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.CreateWorkerAccount;

public sealed record CreateWorkerAccountCommand(string Email, string RoleName, Guid CreatedBy)
    : IRequest<ErrorOr<Guid>>;
