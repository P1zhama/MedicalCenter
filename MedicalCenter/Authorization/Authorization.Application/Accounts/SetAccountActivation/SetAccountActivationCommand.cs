using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.SetAccountActivation;

public sealed record SetAccountActivationCommand(Guid AccountId, bool IsActive)
    : IRequest<ErrorOr<Success>>;
