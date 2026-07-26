using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.DeleteWorkerAccount;

public sealed record DeleteWorkerAccountCommand(Guid AccountId)
    : IRequest<ErrorOr<Success>>;
