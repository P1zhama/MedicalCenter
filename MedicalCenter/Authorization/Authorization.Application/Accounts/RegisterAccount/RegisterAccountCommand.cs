using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.RegisterAccount;

public sealed record RegisterAccountCommand(string Email, string Password)
    : IRequest<ErrorOr<Guid>>;
